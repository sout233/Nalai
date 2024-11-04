// See https://aka.ms/new-console-template for more information

using Nalai.Engine;

const string downloadUrl =
    "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.95.1.24307/VSCodium-win32-x64-1.95.1.24307.zip";

var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\VSCodium.zip";

using var downloader = new Downloader();
_ = downloader.DownloadFileAsync(downloadUrl, path);

Console.WriteLine("Started");

downloader.ProgressChanged += (sender, e) => { Console.WriteLine(e.ProgressPercentage); };

while (true)
{
}