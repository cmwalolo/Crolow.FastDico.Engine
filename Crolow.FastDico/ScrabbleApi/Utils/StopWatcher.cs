using Kalow.Apps.Common.Utils;
using System.Diagnostics;

namespace Crolow.FastDico.ScrabbleApi.Utils
{
    public class StopWatcher : IDisposable
    {
        private Stopwatch stopwatch = new Stopwatch();
        private string message;
        public StopWatcher(string message)
        {
            this.message = message;
            BufferedConsole.WriteLine("Starting : " + message);
            BufferedConsole.WriteLine("-------------------------------------------");
            stopwatch.Start();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            BufferedConsole.WriteLine("-------------------------------------------");
            BufferedConsole.WriteLine("Stopping : " + message + " : " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }
}
