using Kalow.Apps.Common.Utils;
using System.Diagnostics;

namespace Crolow.FastDico.ScrabbleApi.Utils
{
    /// <summary>
    /// Logs the elapsed time of a named operation when disposed.
    /// </summary>
    public class StopWatcher : IDisposable
    {
        private Stopwatch stopwatch = new Stopwatch();
        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopWatcher"/> class and starts timing immediately.
        /// </summary>
        /// <param name="message">Message used in the start and stop log entries.</param>
        public StopWatcher(string message)
        {
            this.message = message;
            BufferedConsole.WriteLine("Starting : " + message);
            BufferedConsole.WriteLine("-------------------------------------------");
            stopwatch.Start();
        }

        /// <summary>
        /// Stops the timer and writes the elapsed milliseconds to the buffered console.
        /// </summary>
        public void Dispose()
        {
            stopwatch.Stop();
            BufferedConsole.WriteLine("-------------------------------------------");
            BufferedConsole.WriteLine("Stopping : " + message + " : " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }
}
