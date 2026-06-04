namespace Crolow.FastDico.ScrabbleApi.Extensions
{
    /// <summary>
    /// Provides formatting helpers for time values.
    /// </summary>
    public static class TimeExtensions
    {
        /// <summary>
        /// Converts a duration in seconds to a minutes-and-seconds display string.
        /// </summary>
        /// <param name="seconds">Duration in seconds.</param>
        /// <returns>A string formatted as <c>MM:SS.ss</c>.</returns>
        public static string ConvertToMMSS(this float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            int minutes = (int)timeSpan.TotalMinutes;
            double remainingSeconds = timeSpan.TotalSeconds % 60;
            return $"{minutes:D2}:{remainingSeconds:00.00}";
        }
    }
}
