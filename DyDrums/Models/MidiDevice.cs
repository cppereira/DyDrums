namespace DyDrums.Models
{
    public class MidiDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name; // O ComboBox vai exibir só o nome
    }
}
