namespace Nalai.Engine;

public class DownloadEvents
{
    public class DownloadSpeedChangedEventArgs(double speed) : EventArgs
    {
        public double Speed { get; set; } = speed; // 单位为字节/秒
    }
}