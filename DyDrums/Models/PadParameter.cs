namespace DyDrums.Models
{
    public class PadParameter
    {
        private Dictionary<int, PadParameter> _padParameters = new();


        public int PadNumber { get; set; }
        public Dictionary<byte, byte> Parameters { get; set; } = new Dictionary<byte, byte>();
    }
}
