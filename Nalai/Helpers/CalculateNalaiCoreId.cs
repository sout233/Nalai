using System.IO;

namespace Nalai.Helpers;

public static class CalculateNalaiCoreId
{
    public static string FromFileNameAndSaveDir(string url,string path)
    {
        var fullPath = Path.Combine(url, path);
        var id = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fullPath));
        return id;
    }
}