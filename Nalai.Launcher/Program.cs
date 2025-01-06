using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace Nalai.Launcher;

internal static class Program
{
    private static void Main(string[] _)
    {
        using var reader = new BinaryReader(Console.OpenStandardInput());

        // 读取消息长度前缀
        var lengthPrefix = reader.ReadBytes(4);
        if (lengthPrefix.Length < 4)
        {
            // 如果没有读到完整的长度前缀，说明没有收到消息
            JobLauncher.LaunchExe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nalai.exe"), []);
            return;
        }

        // 将长度前缀转换为整数
        var length = BitConverter.ToInt32(lengthPrefix, 0);

        // 读取消息体
        var messageBytes = reader.ReadBytes(length);
        if (messageBytes.Length != length)
        {
            // 如果没有读到预期长度的消息体，可能出现了错误
            JobLauncher.LaunchExe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nalai.exe"), []);
            return;
        }

        // 将消息体转换为字符串
        var json = Encoding.UTF8.GetString(messageBytes);

        // 根据接收到的 JSON 数据调用 LaunchExe
        JobLauncher.LaunchExe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nalai.exe"),
            string.IsNullOrEmpty(json) ? Array.Empty<string>() : ["--download", json]);
    }
}