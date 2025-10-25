using System;
using System.Collections.Generic;

namespace Kalow.Apps.Common.Utils
{
    public class BufferedConsole
    {
        static List<KeyValuePair<string, Exception>> consoleBuffer = new List<KeyValuePair<string, Exception>>();

        public static void ClearBuffer()
        {
            consoleBuffer.Clear();
        }

        public static void WriteLine(string message = null, Exception ex = null)
        {
            Console.WriteLine(message);
            consoleBuffer.Add(new KeyValuePair<string, Exception>(message, ex));
        }

        public static List<KeyValuePair<string, Exception>> ReadLog()
        {
            return consoleBuffer;
        }

    }
}
