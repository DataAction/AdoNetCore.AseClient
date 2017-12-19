using System;
using System.Diagnostics;

namespace AdoNetCore.AseClient.Internal
{
    public sealed class Logger
    {
        private static Logger _instance;
        public static Logger Instance
        {
            get
            {
#if RELEASE
                return null;
#else
                return _instance;
#endif
            }
        }

        public static void Enable(bool toConsole = true, bool toDebug = false, bool timestamps = false)
        {
#if DEBUG
            _instance = new Logger
            {
                ToConsole = toConsole,
                ToDebug = toDebug,
                Timestamps = timestamps
            };
#endif
        }

        public static void Disable()
        {
            _instance = null;
        }

        private bool ToConsole { get; set; } = true;
        private bool ToDebug { get; set; }
        private bool Timestamps { get; set; } = true;

        private bool _lineStart = true;

        private Logger() { }

        private string Timestamp => _lineStart && Timestamps ? DateTime.UtcNow.ToString("[yyyy-MM-dd HH:mm:ss] ") : string.Empty;


        public void WriteLine()
        {
            if (ToConsole) Console.WriteLine();
            if (ToDebug) Debug.WriteLine(string.Empty);
        }

        public void WriteLine(string line)
        {
            var formatted = $"{Timestamp}{line}";
            if (ToConsole) Console.WriteLine(formatted);
            if (ToDebug) Debug.WriteLine(formatted);
            _lineStart = true;
        }

        public void Write(string value)
        {
            var formatted = $"{Timestamp}{value}";
            if (ToConsole) Console.Write(formatted);
            if(ToDebug) Debug.Write(formatted);
            _lineStart = false;
        }
    }
}
