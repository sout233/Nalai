namespace Nalai.Helpers;

public static class TimeFormatter
{
    private const int SecondsInMinute = 60;
    private const int SecondsInHour = SecondsInMinute * 60;
    private const int SecondsInDay = SecondsInHour * 24;

    public static string FormatTimeSpan(long seconds)
    {
        if (seconds < SecondsInMinute)
        {
            return $"{seconds}s";
        }
        else if (seconds < SecondsInHour)
        {
            return $"{seconds / SecondsInMinute}m {seconds % SecondsInMinute}";
        }
        else if (seconds < SecondsInDay)
        {
            long minutes = seconds % SecondsInHour;
            return $"{seconds / SecondsInHour}h {minutes / SecondsInMinute}m {minutes % SecondsInMinute}s";
        }
        else
        {
            long hours = seconds % (SecondsInDay);
            long minutes = (hours % SecondsInHour) / SecondsInMinute;
            return $"{seconds / SecondsInDay}d {hours / SecondsInHour}h {minutes}m {hours % SecondsInMinute}s";
        }
    }
    
    public static TimeSpan CalculateRemainingTime(long bytesReceived, long totalBytes, long bytesPerSecond)
    {
        if (bytesPerSecond <= 0)
        {
            throw new ArgumentException("Speed must be greater than zero.", nameof(bytesPerSecond));
        }
        
        long remaining =totalBytes - bytesReceived;
        double remainingTimeSeconds = (double)remaining / bytesPerSecond;

        return TimeSpan.FromSeconds(remainingTimeSeconds);
    }
}
