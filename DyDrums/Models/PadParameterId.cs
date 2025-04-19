namespace DyDrums.Models
{
    public enum PadParameterId : byte
    {
        Type = 0x00,
        Note = 0x01,
        Threshold = 0x02,
        ScanTime = 0x03,
        MaskTime = 0x04,
        Retrigger = 0x05,
        Curve = 0x06,
        CurveForm = 0x07,
        Xtalk = 0x08,
        XtalkGroup = 0x09,
        Channel = 0x0A,
        Gain = 0x0B
    }
}
