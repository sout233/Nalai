using Nalai.Models;
using SqlSugar;

namespace Nalai.Services;

public static class SqlService
{
    public static SqlSugarClient Client { get; set; } = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = "datasource=nalai_main.db",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true
        },
        db => { db.Aop.OnLogExecuting = (sql, pars) => { Console.WriteLine(UtilMethods.GetNativeSql(sql, pars)); }; });
    
    public static event EventHandler<DownloadTask>? OnInsertOrUpdate;

    public static void InitDatabase()
    {
        Client.DbMaintenance.CreateDatabase();

        Client.CodeFirst.InitTables<DownloadTask>();
    }

    public static void InsertOrUpdate(DownloadTask downloadTask)
    {
        Client.Storageable(downloadTask).ExecuteCommand();
        // OnInsertOrUpdate?.Invoke(null, downloadTask);
    }

    public static void InsertOrUpdate(List<DownloadTask> tasks)
    {
        Client.Storageable(tasks).ExecuteCommand();
    }

    public static void Delete(DownloadTask downloadTask)
    {
        Client.Deleteable(downloadTask).ExecuteCommand();
    }

    public static List<DownloadTask?> ReadAll()
    {
       return Client.Queryable<DownloadTask>().ToList();
    }
}