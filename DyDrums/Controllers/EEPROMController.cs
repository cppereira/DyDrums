using DyDrums.Services;

namespace DyDrums.Controllers
{
    public class EEPROMController
    {
        private readonly EEPROMManager _eepromManager;
        private readonly PadManager _padManager;
        private readonly MainForm _view;

        public EEPROMController(EEPROMManager eepromManager, PadManager padManager, MainForm view)
        {
            _eepromManager = eepromManager;
            _padManager = padManager;
            _view = view;
        }

        public void HandleSysexMessages(List<byte[]> messages)
        {
            _padManager.ProcessSysex(messages); // ✔️ Perfeito! PadManager faz o parse
            _view.Invoke(() => _view.RefreshPadGrid());
        }
    }
}
