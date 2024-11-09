using Nalai.CoreConnector;
using Nalai.CoreConnector.Models;

namespace Nalai.Models;

public class CoreTask
{
    GetStatusResult StatusResult { get; set; }
    string FileName { get; set; }
    string SavePath { get; set; }
    string Url { get; set; }
    string Id { get; set; }

    public CoreTask(string id, string url, string savePath, string fileName)
    {
        Id = id;
        Url = url;
        SavePath = savePath;
        FileName = fileName;
        
        
    }

    public void StartListen()
    {
        Task.Run(() =>
        {
            while (StatusResult.Status != "Finished")
            {
                var result = PreCore.GetStatusAsync(Id);
                
            }
        });
    }
}