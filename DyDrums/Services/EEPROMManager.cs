namespace DyDrums.Services
{
    public class EEPROMManager
    {
        private readonly SerialManager serialManager;

        public EEPROMManager(SerialManager serialManager)
        {
            this.serialManager = serialManager;
        }
    }
}
