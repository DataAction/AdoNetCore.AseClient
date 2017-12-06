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

        public static void Enable(bool toConsole = true, bool toDebug = false)
        {
#if DEBUG
            _instance = new Logger
            {
                ToConsole = toConsole,
                ToDebug = toDebug
            };
#endif
        }

        public static void Disable()
        {
            _instance = null;
        }

        private bool ToConsole { get; set; } = true;
        private bool ToDebug { get; set; } = false;

        private Logger() { }

        public void WriteLine()
        {
            if (ToConsole) Console.WriteLine();
            if (ToDebug) Debug.WriteLine(string.Empty);
        }

        public void WriteLine(string line)
        {
            if (ToConsole) Console.WriteLine(line);
            if (ToDebug) Debug.WriteLine(line);
        }

        public void Write(string value)
        {
            if(ToConsole) Console.Write(value);
            if(ToDebug) Debug.Write(value);
        }
    }
}
