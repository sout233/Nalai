using Nalai.CoreConnector.Models;

namespace Nalai.Helpers;

public static class TimeFormatter
{
    private const int SecondsInMinute = 60;
    private const int SecondsInHour = SecondsInMinute * 60;
    private const int SecondsInDay = SecondsInHour * 24;

    public static string FormatSecondsReadable(long seconds)
    {
        if (seconds < SecondsInMinute)
        {
            return $"{seconds}s";
        }

        if (seconds < SecondsInHour)
        {
            return $"{seconds / SecondsInMinute}m {seconds % SecondsInMinute}";
        }

        if (seconds < SecondsInDay)
        {
            var minutes = seconds % SecondsInHour;
            return $"{seconds / SecondsInHour}h {minutes / SecondsInMinute}m {minutes % SecondsInMinute}s";
        }
        else
        {
            var hours = seconds % (SecondsInDay);
            var minutes = (hours % SecondsInHour) / SecondsInMinute;
            return $"{seconds / SecondsInDay}d {hours / SecondsInHour}h {minutes}m {hours % SecondsInMinute}s";
        }
    }
    
    public static string FormatTimeSpanReadable(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 60)
        {
            return $"{timeSpan.Seconds}s";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        if (timeSpan.TotalHours < 24)
        {
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }

    public static TimeSpan CalculateRemainingTime(long bytesReceived, long totalBytes, long bytesPerSecond)
    {
        if (bytesPerSecond <= 0)
        {
            return TimeSpan.Zero;
        }

        var remaining = totalBytes - bytesReceived;
        var remainingTimeSeconds = (double)remaining / bytesPerSecond;

        return TimeSpan.FromSeconds(remainingTimeSeconds);
    }

    public static DateTimeOffset ConvertRustSystemTime(RustSystemTime systemTime)
    {
        var secsSinceEpoch = systemTime.SecsSinceEpoch;
        var nanosSinceEpoch = systemTime.NanosSinceEpoch;

        // 将秒数转换为 TimeSpan
        var secondsSpan = TimeSpan.FromSeconds(secsSinceEpoch);
        // 将纳秒数转换为 TimeSpan
        var nanosecondsSpan = TimeSpan.FromTicks(nanosSinceEpoch / 100); // .NET 中的 Ticks 是 100 纳秒

        // 计算总时间跨度
        var totalSpan = secondsSpan + nanosecondsSpan;

        // 获取 UNIX 纪元时间
        var unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // 计算最终的时间点
        var dateTime = unixEpoch + totalSpan;

        return dateTime;
    }
    
    public static string FormatRustSystemTime(RustSystemTime systemTime)
    {
        var dateTime = ConvertRustSystemTime(systemTime);
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}