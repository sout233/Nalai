namespace Nalai.Engine.Helpers;

public static class DownloadHelpers
{
    public static string GetTempFilePath(string outputPath, int chunkIndex)
    {
        var tempDir = Path.GetDirectoryName(outputPath)!;
        var tempPrefix = Path.GetFileNameWithoutExtension(outputPath);
        var tempExt = Path.GetExtension(outputPath);
        return Path.Combine(tempDir, $"{tempPrefix}.part{chunkIndex}{tempExt}");
    }

    public static async Task MergeChunksAsync(string outputPath, long contentLength, int chunkCount)
    {
        var tempDir = Path.GetDirectoryName(outputPath)!;
        var tempPrefix = Path.GetFileNameWithoutExtension(outputPath);
        var tempExt = Path.GetExtension(outputPath);

        using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        for (var i = 0; i < chunkCount; i++)
        {
            var tempFilePath = GetTempFilePath(outputPath, i);
            using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fileStream.CopyToAsync(outputStream);
            File.Delete(tempFilePath);
        }
    }
}