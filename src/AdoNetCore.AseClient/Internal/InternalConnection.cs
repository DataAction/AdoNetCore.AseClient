using System;
using System.IO;
using System.Text;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class InternalConnection : IInternalConnection
    {
        private readonly ConnectionParameters _parameters;
        private readonly ISocket _socket;
        private readonly ITokenParser _tokenParse;

        private int _packetSize = 512;
        private int _headerSize = 8;

        public InternalConnection(ConnectionParameters parameters, ISocket socket, ITokenParser tokenParse)
        {
            _parameters = parameters;
            _socket = socket;
            _tokenParse = tokenParse;
        }

        public void Connect()
        {
            //socket is established already
            //login
            var loginPacket = new LoginPacket(_parameters.ClientHostName, _parameters.Username, _parameters.Password, _parameters.ProcessId, _parameters.ApplicationName, _parameters.Server, "us_english", _parameters.Charset, "ADO.NET", _packetSize, new CapabilityToken());

            using (var ms = new MemoryStream())
            {
                loginPacket.Write(ms, Encoding.ASCII);
                ms.Seek(0, SeekOrigin.Begin);

                while (ms.Position < ms.Length)
                {
                    //split into chunks and send over the wire
                    var buffer = new byte[_packetSize];
                    var template = loginPacket.HeaderTemplate;
                    Array.Copy(template, buffer, template.Length);
                    var copied = ms.Read(buffer, template.Length, buffer.Length - template.Length);
                    var chunkLength = template.Length + copied;
                    buffer[1] = (byte)(ms.Position >= ms.Length ? BufferStatus.TDS_BUFSTAT_EOM : BufferStatus.TDS_BUFSTAT_NONE); //todo: set other statuses?
                    buffer[2] = (byte)(chunkLength >> 8);
                    buffer[3] = (byte)chunkLength;

                    if (chunkLength == _packetSize)
                    {
                        _socket.Send(buffer);
                    }
                    else
                    {
                        var temp = new byte[chunkLength];
                        Array.Copy(buffer, temp, chunkLength);
                        _socket.Send(temp);
                    }
                }
            }

            using (var ms = new MemoryStream())
            {
                var buffer = new byte[_packetSize];
                var received = _socket.Receive(buffer);
                BufferType type = BufferType.TDS_BUF_NONE;
                while (received > 0)
                {
                    if (type == BufferType.TDS_BUF_NONE)
                    {
                        type = (BufferType) buffer[0];
                    }

                    if (received > _headerSize)
                    {
                        ms.Write(buffer, _headerSize, received - _headerSize);
                    }

                    //todo: fix this, we may need to read the header to determine how many bytes left
                    if (received < _packetSize)
                    {
                        received = 0;
                    }
                    else
                    {
                        received = _socket.Receive(buffer);
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                Console.WriteLine(HexDump.Dump(ms.ToArray()));

                _tokenParse.Parse(ms, Encoding.ASCII);
                //we may not actually care what kind of BufferType we get back
                /*if (type == BufferType.TDS_BUF_RESPONSE)
                {
                    var response = new ResponsePacket();
                    //response.Read(ms, Encoding.ASCII);
                }*/
            }
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Database = databaseName;

            //Send a USE X command
            throw new NotImplementedException();
        }

        public string Database { get; private set; }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
