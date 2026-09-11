namespace PicRepo.Client.Helper;

public class DataUnitHelper
{
    private static readonly ulong TbLength = Convert.ToUInt64(Math.Pow(1024, 4));
    private static readonly ulong GbLength = Convert.ToUInt64(Math.Pow(1024, 3));
    private static readonly ulong MbLength = Convert.ToUInt64(Math.Pow(1024, 2));
    private static readonly ulong KbLength = 1024;

    private static readonly ulong TbpsLength = Convert.ToUInt64(Math.Pow(10, 12));
    private static readonly ulong GbpsLength = Convert.ToUInt64(Math.Pow(10, 9));
    private static readonly ulong MbpsLength = Convert.ToUInt64(Math.Pow(10, 6));
    private static readonly ulong KbpsLength = Convert.ToUInt64(Math.Pow(10, 3));

    public static string GetLength(ulong length)
    {
        if (length >= TbLength)
        {
            return (length * 1.0 / TbLength).ToString("0.##") + " TB";
        }

        if (length >= GbLength)
        {
            return (length * 1.0 / (GbLength)).ToString("0.##") + " GB";
        }

        if (length >= MbLength)
        {
            return (length * 1.0 / (MbLength)).ToString("0.##") + " MB";
        }

        if (length >= KbLength)
        {
            return (length * 1.0 / KbLength).ToString("0.##") + " KB";
        }

        return length + " B";
    }

    public static string GetTransferRate(double rate)
    {
        if (rate >= TbpsLength)
        {
            return (rate * 1.0 / TbpsLength).ToString("0.###") + " Tbps";
        }

        if (rate >= GbpsLength)
        {
            return (rate * 1.0 / (GbpsLength)).ToString("0.###") + " Gbps";
        }

        if (rate >= MbpsLength)
        {
            return (rate * 1.0 / (MbpsLength)).ToString("0.###") + " Mbps";
        }

        if (rate >= KbpsLength)
        {
            return (rate * 1.0 / KbpsLength).ToString("0.###") + " Kbps";
        }

        return rate.ToString("0.###") + " bps";
    }
}