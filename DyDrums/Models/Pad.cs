namespace DyDrums.Models
{
    public class Pad
    {
        public int Pin { get; set; }
        public int Type { get; set; }
        public string Name { get; set; } = "";
        public int Note { get; set; }
        public int Threshold { get; set; }
        public int ScanTime { get; set; }
        public int MaskTime { get; set; }
        public int Retrigger { get; set; }
        public int Curve { get; set; }
        public int CurveForm { get; set; }
        public int Xtalk { get; set; }
        public int XtalkGroup { get; set; }
        public int Channel { get; set; }
        public int Gain { get; set; }

        public Pad()
        {
            Pin = -1; // ou 0, ou algum valor padrão que grite “INICIALIZADO ERRADO”
        }
        public Pad(int id)
        {
            Pin = id;
        }


        public Pad Clone()
        {
            return (Pad)this.MemberwiseClone();
        }
    }
}
