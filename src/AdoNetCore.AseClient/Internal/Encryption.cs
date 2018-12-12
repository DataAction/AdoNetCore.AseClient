using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    internal static class Encryption
    {
        public static IToken[] BuildEncrypt2Tokens(byte[] encryptedPassword)
        {
            var pwdFormat = new FormatItem
            {
                DataType = TdsDataType.TDS_LONGBINARY,
                Length = 512
            };
            var remPwdVarcharFormat = new FormatItem
            {
                DataType = TdsDataType.TDS_VARCHAR,
                Length = 255,
                IsNullable = true
            };

            return new IToken[]
            {
                new MessageToken
                {
                    Status = MessageToken.MsgStatus.TDS_MSG_HASARGS,
                    MessageId = MessageToken.MsgId.TDS_MSG_SEC_LOGPWD2
                },
                new ParameterFormatToken
                {
                    Formats = new[]
                    {
                        pwdFormat,
                        remPwdVarcharFormat,
                        pwdFormat
                    }
                },
                new ParametersToken
                {
                    Parameters = new[]
                    {
                        new ParametersToken.Parameter
                        {
                            Value = encryptedPassword,
                            Format = pwdFormat
                        },
                        new ParametersToken.Parameter
                        {
                            Value = DBNull.Value,
                            Format = remPwdVarcharFormat
                        },
                        new ParametersToken.Parameter
                        {
                            Value = encryptedPassword,
                            Format = pwdFormat
                        }
                    }
                },
                new DoneToken
                {
                    Count = 0,
                    Status = DoneToken.DoneStatus.TDS_DONE_FINAL,
                    TransactionState = TranState.TDS_NOT_IN_TRAN
                }
            };
        }

        public static IToken[] BuildEncrypt3Tokens(byte[] encryptedPassword)
        {
            var pwdFormat = new FormatItem
            {
                DataType = TdsDataType.TDS_LONGBINARY,
                Length = 512
            };
            var remPwdVarcharFormat = new FormatItem
            {
                DataType = TdsDataType.TDS_VARCHAR,
                Length = 255,
                IsNullable = true
            };

            return new IToken[]
            {
                new MessageToken
                {
                    Status = MessageToken.MsgStatus.TDS_MSG_HASARGS,
                    MessageId = MessageToken.MsgId.TDS_MSG_SEC_LOGPWD3
                },
                new ParameterFormatToken
                {
                    Formats = new[]
                    {
                        pwdFormat
                    }
                },
                new ParametersToken
                {
                    Parameters = new[]
                    {
                        new ParametersToken.Parameter
                        {
                            Value = encryptedPassword,
                            Format = pwdFormat
                        }
                    }
                },
                new MessageToken
                {
                    Status = MessageToken.MsgStatus.TDS_MSG_HASARGS,
                    MessageId = MessageToken.MsgId.TDS_MSG_SEC_REMPWD3
                },
                new ParameterFormatToken
                {
                    Formats = new[]
                    {
                        remPwdVarcharFormat,
                        pwdFormat
                    }
                },
                new ParametersToken
                {
                    Parameters = new[]
                    {
                        new ParametersToken.Parameter
                        {
                            Value = DBNull.Value,
                            Format = remPwdVarcharFormat
                        },
                        new ParametersToken.Parameter
                        {
                            Value = encryptedPassword,
                            Format = pwdFormat
                        }
                    }
                }
            };
        }

        public static byte[] EncryptPassword2(int suite, byte[] rsaKey, byte[] passwordBytes)
        {
            Logger.Instance?.WriteLine($"Cipher Suite: {suite}");
            Logger.Instance?.WriteLine($"RsaKey [{rsaKey.Length}]: {ByteArrayToHexString(rsaKey)}");

            byte[] encryptedPassword;
            using (var rsa = RSA.Create())
            {
                var rsaParams = ReadPublicKey(rsaKey);
                Logger.Instance?.WriteLine($"RSA Mod [{rsaParams.Modulus.Length}]: {ByteArrayToHexString(rsaParams.Modulus)}");
                Logger.Instance?.WriteLine($"RSA Exp [{rsaParams.Exponent.Length}]: {ByteArrayToHexString(rsaParams.Exponent)}");
                rsa.ImportParameters(rsaParams);

                encryptedPassword = rsa.Encrypt(passwordBytes, RSAEncryptionPadding.OaepSHA1);
            }

            Logger.Instance?.WriteLine($"Encrypted Bytes [{encryptedPassword.Length}]: {ByteArrayToHexString(encryptedPassword)}");
            return encryptedPassword;
        }

        public static byte[] EncryptPassword3(int suite, byte[] rsaKey, byte[] nonce, byte[] passwordBytes)
        {
            Logger.Instance?.WriteLine($"Cipher Suite: {suite}");
            Logger.Instance?.WriteLine($"RsaKey [{rsaKey.Length}]: {ByteArrayToHexString(rsaKey)}");
            Logger.Instance?.WriteLine($"Nonce [{nonce.Length}]: {ByteArrayToHexString(nonce)}");

            byte[] encryptedPassword;
            using (var rsa = RSA.Create())
            {
                var rsaParams = ReadPublicKey(rsaKey);
                Logger.Instance?.WriteLine($"RSA Mod [{rsaParams.Modulus.Length}]: {ByteArrayToHexString(rsaParams.Modulus)}");
                Logger.Instance?.WriteLine($"RSA Exp [{rsaParams.Exponent.Length}]: {ByteArrayToHexString(rsaParams.Exponent)}");
                rsa.ImportParameters(rsaParams);

                var noncedBytes = new byte[passwordBytes.Length + nonce.Length];
                Array.Copy(nonce, 0, noncedBytes, 0, nonce.Length);
                Array.Copy(passwordBytes, 0, noncedBytes, nonce.Length, passwordBytes.Length);
                encryptedPassword = rsa.Encrypt(noncedBytes, RSAEncryptionPadding.OaepSHA1);
            }

            Logger.Instance?.WriteLine($"Encrypted Bytes [{encryptedPassword.Length}]: {ByteArrayToHexString(encryptedPassword)}");
            return encryptedPassword;
        }

        private static RSAParameters ReadPublicKey(byte[] publicKey)
        {
            Logger.Instance?.WriteLine($"{nameof(ReadPublicKey)} {publicKey.Length} bytes");
            var keyFile = Encoding.ASCII.GetString(publicKey);
            Logger.Instance?.WriteLine(keyFile);
            var strippedKeyFile = keyFile
                .Replace("\n", string.Empty)
                .Replace("\0", string.Empty)
                .Replace("-----BEGIN RSA PUBLIC KEY-----", string.Empty)
                .Replace("-----END RSA PUBLIC KEY-----", string.Empty);

            var der = Convert.FromBase64String(strippedKeyFile);
            Logger.Instance?.WriteLine($"DER ASN.1 encoded key is {der.Length} bytes");

            /* Assume the format is
            RSAPublicKey ::= SEQUENCE {
                 modulus            INTEGER,
                 publicExponent     INTEGER
            }*/

            using (var ms = new MemoryStream(der))
            {
                var seq = ms.ReadByte();
                Debug.Assert(seq == 0x30);
                var seqRemainingBytes = ReadDerLen(ms);
                using (var psSeq = new ReadablePartialStream(ms, seqRemainingBytes))
                {
                    return new RSAParameters
                    {
                        Modulus = SkipFirstByteIfZero(ReadDerInteger(psSeq)),
                        Exponent = ReadDerInteger(psSeq)
                    };
                }
            }
        }

        private static byte[] SkipFirstByteIfZero(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return bytes;
            }

            if (bytes[0] != 0)
            {
                return bytes;
            }

            var newBytes = new byte[bytes.Length - 1];

            if (newBytes.Length > 0)
            {
                Array.Copy(bytes, 1, newBytes, 0, newBytes.Length);
            }

            return newBytes;
        }

        private static uint ReadDerLen(Stream s)
        {
            var len = s.ReadByte();
            //short form length?
            if (len < 0x80)
            {
                return (uint)len;
            }
            //long form length, bits 1-7 indicate how many octets contribute to the length
            var count = len & 0x7F;
            if (count > 4)
            {
                throw new NotSupportedException("Too many bytes!");
            }

            var bytes = new byte[] { 0, 0, 0, 0 };
            s.Read(bytes, 0, count);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static byte[] ReadDerInteger(Stream s)
        {
            var type = s.ReadByte();
            Debug.Assert(type == 0x02);

            var len = ReadDerLen(s);
            var bytes = new byte[len];
            s.Read(bytes, 0, bytes.Length);

            return bytes;
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
