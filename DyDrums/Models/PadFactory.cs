using DyDrums.Models;
public static class PadFactory
{
    public static Pad FromParameters(int id, Dictionary<byte, int> parameters)
    {
        return new Pad
        {
            //Parâmetros correspondentes ao firmware do Arduino, não alterar...
            //Removi parametros nao utilizados do firmware original para se encaixar ao meu uso
            //Antes pulada do 0x08 para o 0x0D, pois 0x09, 0x0A, 0x0B, 0x0C
            //eram parametros que nao utilizo, como DualSensor, ChokeNote, etc...

            Id = id, // Index/PIN

            Type = Get(parameters, 0x00),
            Note = Get(parameters, 0x01),
            Threshold = Get(parameters, 0x02),
            ScanTime = Get(parameters, 0x03),
            MaskTime = Get(parameters, 0x04),
            Retrigger = Get(parameters, 0x05),
            Curve = Get(parameters, 0x06),
            CurveForm = Get(parameters, 0x07),
            Xtalk = Get(parameters, 0x08),
            XtalkGroup = Get(parameters, 0x09),
            Channel = Get(parameters, 0x0A),
            Gain = Get(parameters, 0x0B),
            PadName = null

        };
    }

    private static int Get(Dictionary<byte, int> dict, byte key)
    {
        return dict.TryGetValue(key, out int value) ? value : 0;
    }
}
