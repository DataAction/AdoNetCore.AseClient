using System;
using System.IO;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Internal;

namespace AdoNetCore.AseClient.Token
{
    internal class OptionCommandToken : IToken
    {
        public TokenType Type => TokenType.TDS_OPTIONCMD;

        public CommandType Command { get; private set; }

        public OptionType Option { get; private set; }

        public byte[] Arguments { get; set; }

        private OptionCommandToken() { }

        public void Write(Stream stream, DbEnvironment env)
        {
            Logger.Instance?.Write($"-> {Type}: {Command}, {Option},");
            foreach (var b in Arguments)
            {
                Logger.Instance?.Write($" {b:x2}");
            }
            stream.WriteByte((byte)Type);
            stream.WriteShort((short)(3 + Arguments.Length));
            stream.WriteByte((byte)Command);
            stream.WriteByte((byte)Option);
            stream.WriteBytePrefixedByteArray(Arguments);
        }

        public void Read(Stream stream, DbEnvironment env, IFormatToken previousFormatToken)
        {
            var remainingLength = stream.ReadShort();
            using (var ts = new ReadablePartialStream(stream, remainingLength))
            {
                Command = (CommandType)ts.ReadByte();
                Option = (OptionType)ts.ReadByte();
                Arguments = ts.ReadByteLengthPrefixedByteArray();
            }
        }

        public static OptionCommandToken Create(Stream stream, DbEnvironment env, IFormatToken previous)
        {
            var t = new OptionCommandToken();
            t.Read(stream, env, previous);
            return t;
        }

        public static OptionCommandToken CreateSetTextSize(int textSize)
        {
            return new OptionCommandToken
            {
                Arguments = BitConverter.GetBytes(textSize),
                Command = CommandType.TDS_OPT_SET,
                Option = OptionType.TDS_OPT_TEXTSIZE
            };
        }

        public static OptionCommandToken CreateGet(OptionType option)
        {
            return new OptionCommandToken
            {
                Arguments = new byte[0],
                Command = CommandType.TDS_OPT_LIST,
                Option = option
            };
        }

        public enum CommandType : byte
        {
            /// <summary>
            /// Set an option.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_SET = 1,
            /// <summary>
            /// Set option to its default value.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_DEFAULT = 2,
            /// <summary>
            /// Request current setting of a specific option.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_LIST = 3,
            /// <summary>
            /// Report current setting of a specific option.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_INFO = 4,
        }

        public enum OptionType : byte
        {
            /// <summary>
            /// Used to specify no option.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_UNUSED = 0,
            /// <summary>
            /// Set first day of week.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_DATEFIRST = 1,
            /// <summary>
            /// Set maximum text size.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_TEXTSIZE = 2,
            /// <summary>
            /// Return server time statistics.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_STAT_TIME = 3,
            /// <summary>
            /// Return server I/O statistics.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_STAT_IO = 4,
            /// <summary>
            /// Set maximum row count to return.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ROWCOUNT = 5,
            /// <summary>
            /// Change national language.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_NATLANG = 6,
            /// <summary>
            /// Set date format.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_DATEFORMAT = 7,
            /// <summary>
            /// Transaction isolation level.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ISOLATION = 8,
            /// <summary>
            /// Set authority level on.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_AUTHON = 9,
            /// <summary>
            /// Change character set.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CHARSET = 10,
            /// <summary>
            /// Show execution plan.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_SHOWPLAN = 13,
            /// <summary>
            /// Do not execute query.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_NOEXEC = 14,
            /// <summary>
            /// Set arithmetic exception handling.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ARITHIGNOREON = 15,
            /// <summary>
            /// Set arithmetic abort handling.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ARITHABORTON = 17,
            /// <summary>
            /// Parse the query only. Return error messages.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_PARSEONLY = 18,
            /// <summary>
            /// Return trigger data.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_GETDATA = 20,
            /// <summary>
            /// Do not return done count.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_NOCOUNT = 21,
            /// <summary>
            /// Forces substitution order for joins in the order of the tables provided in this option.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_FORCEPLAN = 23,
            /// <summary>
            /// Send format information only.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_FORMATONLY = 24,
            /// <summary>
            /// Set chained transaction mode.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CHAINXACTS = 25,
            /// <summary>
            /// Close all open cursors at end of transaction.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CURCLOSEONXACT = 26,
            /// <summary>
            /// Enable FIPs flagging.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_FIPSFLAG = 27,
            /// <summary>
            /// Return resolution trees.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_RESTREES = 28,
            /// <summary>
            /// Turn on explicit identity.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_IDENTITYON = 29,
            /// <summary>
            /// Set session label @@curread.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CURREAD = 30,
            /// <summary>
            /// Set session label @@curwrite.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CURWRITE = 31,
            /// <summary>
            /// Turn off explicit identity.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_IDENTITYOFF = 32,
            /// <summary>
            /// Turn authority off.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_AUTHOFF = 33,
            /// <summary>
            /// Support ANSI null data.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ANSINULL = 34,
            /// <summary>
            /// Quoted identifiers.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_QUOTED_IDENT = 35,
            /// <summary>
            /// Check permissions on search columns for update clause.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_ANSIPERM = 36,
            /// <summary>
            /// ANSI string right trunc.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_STR_RTRUNC = 37,
            /// <summary>
            /// Set Sort-Merge for session.
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_SORTMERGE = 38,
            /// <summary>
            /// Set JTC for session
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_JTC = 39,
            /// <summary>
            /// Set Client Real Name
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CLIENTREALNAME = 40,
            /// <summary>
            /// Set Client Host Name
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CLIENTHOSTNAME = 41,
            /// <summary>
            /// Set Client Application Name
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CLIENTAPPLNAME = 42,
            /// <summary>
            /// Turn on explicit update identity on table
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_IDENTITYUPD_ON = 43,
            /// <summary>
            /// Turn off explicit update identity on table
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_IDENTITYUPD_OFF = 44,
            /// <summary>
            /// Turn on/off “nodata”option
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_NODATA = 45,
            /// <summary>
            /// Turn on/off ciphertext (column encryption)
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_CIPHERTEXT = 46,
            /// <summary>
            /// Expose Functional Indexes
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_SHOW_FI = 47,
            /// <summary>
            /// Hide/Show Virtual Computed Columns
            /// </summary>
            // ReSharper disable once InconsistentNaming
            TDS_OPT_HIDE_VCC = 48,
        }
    }
}
