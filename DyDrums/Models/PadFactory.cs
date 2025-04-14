using DyDrums.Models;
public static class PadFactory
{
    public static Pad FromParameters(int id, Dictionary<byte, int> parameters)
    {
        return new Pad
        {
            //Parâmetros lidos do Arduino, não alterar...
            Id = id,
            Type = Get(parameters, 0x0D),
            Name = Get(parameters, 0x0C).ToString(),
            Note = Get(parameters, 0x00),
            Threshold = Get(parameters, 0x01),
            ScanTime = Get(parameters, 0x02),
            MaskTime = Get(parameters, 0x03),
            Retrigger = Get(parameters, 0x04),
            Curve = Get(parameters, 0x05),
            CurveForm = Get(parameters, 0x08),
            Xtalk = Get(parameters, 0x06),
            XtalkGroup = Get(parameters, 0x07),
            Channel = Get(parameters, 0x0E),
            Gain = Get(parameters, 0x0F)
        };
    }

    private static int Get(Dictionary<byte, int> dict, byte key)
    {
        return dict.TryGetValue(key, out int value) ? value : 0;
    }
}
