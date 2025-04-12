using System.Diagnostics;
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
            Debug.WriteLine("Sysex recebido!"); // ← A prova de vida!

            _padManager.ProcessSysex(messages); // Parse e atualização dos Pads
            _view.Invoke(() =>
            {
                _view.RefreshPadGrid(); // Atualiza a interface
            });
        }
    }
}
