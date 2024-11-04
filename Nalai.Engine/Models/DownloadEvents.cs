namespace Nalai.Engine.Models;

public abstract class DownloadEvents
{
    public class DownloadSpeedChangedEventArgs(double speed) : EventArgs
    {
        public double Speed { get; } = speed; // 单位为字节/秒
    }
}