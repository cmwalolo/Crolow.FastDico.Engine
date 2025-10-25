namespace Crolow.FastDico.ScrabbleApi.Extensions
{
    public static class TimeExtensions
    {
        public static string ConvertToMMSS(this float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            int minutes = (int)timeSpan.TotalMinutes;
            double remainingSeconds = timeSpan.TotalSeconds % 60;
            return $"{minutes:D2}:{remainingSeconds:00.00}";
        }
    }
}
