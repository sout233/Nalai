namespace Nalai.Helpers;

public class GetUrlInfo
{
    public static string GetFileName(string url)
    {
        string[] parts = url.Split('/');
        return parts[^1];
    }
}