namespace Nalai.Helpers;

public static class ByteSizeFormatter
{
    private const long KiloByte = 1024;
    private const long MegaByte = KiloByte * 1024;
    private const long GigaByte = MegaByte * 1024;
    private const long TeraByte = GigaByte * 1024;

    public static string FormatSize(long bytes)
    {
        double formattedSize;
        string unit;

        if (bytes >= TeraByte)
        {
            formattedSize = (double)bytes / TeraByte;
            unit = "TB";
        }
        else if (bytes >= GigaByte)
        {
            formattedSize = (double)bytes / GigaByte;
            unit = "GB";
        }
        else if (bytes >= MegaByte)
        {
            formattedSize = (double)bytes / MegaByte;
            unit = "MB";
        }
        else if (bytes >= KiloByte)
        {
            formattedSize = (double)bytes / KiloByte;
            unit = "KB";
        }
        else
        {
            formattedSize = bytes;
            unit = "Bytes";
        }

        return $"{formattedSize:0.##} {unit}";
    }
}
