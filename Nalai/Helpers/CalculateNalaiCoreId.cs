using System.IO;

namespace Nalai.Helpers;

public static class CalculateNalaiCoreId
{
    public static string FromFileNameAndSaveDir(string fileName, string saveDir)
    {
        var fullPath = Path.Combine(saveDir, fileName);
        var id = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fullPath));
        return id;
    }
}