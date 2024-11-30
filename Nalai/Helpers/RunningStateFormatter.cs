using Nalai.CoreConnector.Models;

namespace Nalai.Helpers;

public static class RunningStateFormatter
{
    public static string Format(HealthStatus s)
    {
        return s switch
        {
            HealthStatus.Running => "Success",
            HealthStatus.Unknown => "Critical",
            _ => ""
        };
    }
        
}