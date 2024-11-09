// See https://aka.ms/new-console-template for more information

using Nalai.CoreConnector;

var preCore = new PreCore();

var id = await preCore.StartAsync("https://mirrors.tuna.tsinghua.edu.cn/debian-cd/current/amd64/iso-cd/debian-12.7.0-amd64-netinst.iso","C:/download");
Console.WriteLine(id.Id);

while (true)
{
    var status = await preCore.GetStatusAsync(id.Id);
    Console.WriteLine($"Downloaded:{status.DownloadedBytes} Total:{status.TotalSize} FileName:{status.FileName} Status:{status.Status}");
    await Task.Delay(200);
}