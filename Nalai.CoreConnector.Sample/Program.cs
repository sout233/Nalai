using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;

var preCore = new PreCore();

const string Url2 = "https://mirrors.tuna.tsinghua.edu.cn/debian/dists/Debian10.13/ChangeLog";
// var id = await preCore.StartAsync("https://mirrors.tuna.tsinghua.edu.cn/debian-cd/current/amd64/iso-cd/debian-12.7.0-amd64-netinst.iso","C:/download");
var id = await preCore.StartAsync(Url2, "C:/download");
Console.WriteLine(id.Id);

Task.Run(async () =>
{
    while (true)
    {
        var status = await preCore.GetStatusAsync(id.Id);
        if (status.TotalSize > 0)
        {
            var progress = (((float)status.DownloadedBytes / status.TotalSize) * 100).ToString("0.00")+"%";
            Console.WriteLine($"Downloaded:{status.DownloadedBytes} Total:{status.TotalSize} FileName:{status.FileName} Status:{status.Status} Progress:{progress}");
        }
        await Task.Delay(500);
    }
});

await Task.Delay(5000);

var result = await preCore.StopAsync(id.Id);

Console.WriteLine(result.IsSuccess);

Environment.Exit(0);