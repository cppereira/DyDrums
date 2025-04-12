using DyDrums.Models;

namespace DyDrums.Services
{
    public class EEPROMManager
    {
        private readonly SerialManager serialManager;

        public EEPROMManager(SerialManager serialManager)
        {
            this.serialManager = serialManager;
        }

        public List<Pad> ParseSysex(List<byte[]> sysexMessages)
        {
            var padMap = new Dictionary<int, Pad>();

            foreach (var message in sysexMessages)
            {
                if (message.Length < 7 || message[0] != 0xF0 || message[^1] != 0xF7)
                    continue;

                int pin = message[3];
                int param = message[4];
                int value = message[5];

                if (pin < 0 || pin > 14)
                    continue;

                if (!padMap.ContainsKey(pin))
                    padMap[pin] = new Pad { Pin = pin };

                var pad = padMap[pin];

                switch (param)
                {
                    case 0x00: pad.Note = value; break;
                    case 0x01: pad.Threshold = value; break;
                    case 0x02: pad.ScanTime = value; break;
                    case 0x03: pad.MaskTime = value; break;
                    case 0x04: pad.Retrigger = value; break;
                    case 0x05: pad.Curve = value; break;
                    case 0x06: pad.Xtalk = value; break;
                    case 0x07: pad.XtalkGroup = value; break;
                    case 0x08: pad.CurveForm = value; break;
                    case 0x0D: pad.Type = value; break;
                    case 0x0E: pad.Channel = value; break;
                    // Faltou isso aqui ó:
                    case 0x0F: pad.Gain = value; break;
                }
            }

            return padMap.Values.ToList();
        }
    }
}
