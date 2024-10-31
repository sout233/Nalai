using Nalai.Models;
using SqlSugar;

namespace Nalai.Services;

public static class SqlService
{
    public static void InitDatabase()
    {
        var client = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "datasource=nalai_main.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            },
            db => {
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));
                    //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                    //Console.WriteLine(UtilMethods.GetSqlString(DbType.SqlServer,sql,pars))
                };
                //注意多租户 有几个设置几个
                //db.GetConnection(i).Aop
            });
        
        client.DbMaintenance.CreateDatabase();
        
        client.CodeFirst.InitTables<DownloadTask>();
    }
}