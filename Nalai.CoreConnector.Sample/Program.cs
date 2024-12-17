using Nalai.CoreConnector;


const string Url2 = "https://mirrors.tuna.tsinghua.edu.cn/debian/dists/Debian10.13/ChangeLog";
// var id = await preCore.StartAsync("https://mirrors.tuna.tsinghua.edu.cn/debian-cd/current/amd64/iso-cd/debian-12.7.0-amd64-netinst.iso","C:/download");
var id = await CoreService.SendStartMsgAsync(Url2, "C:/download","","");
Console.WriteLine(id.Id);

// Task.Run(async () =>
// {
//     while (true)
//     {
//         var status = await PreCore.GetStatusAsync(id.Id);
//         if (status.TotalBytes > 0)
//         {
//             var progress = (((float)status.DownloadedBytes / status.TotalBytes) * 100).ToString("0.00")+"%";
//             Console.WriteLine($"Downloaded:{status.DownloadedBytes} Total:{status.TotalBytes} FileName:{status.FileName} Status:{status.StatusText} Progress:{progress}");
//         }
//         await Task.Delay(500);
//     }
// });

await Task.Delay(5000);

var result = await CoreService.SendStopMsgAsync(id.Id);

// Console.WriteLine(result.IsSuccess);

Environment.Exit(0);