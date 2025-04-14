using System.Text.Json;
using DyDrums.Models;
using DyDrums.Services;

public class PadManager
{
    public event Action<List<Pad>>? PadsUpdated;
    private readonly string _configPath = "pads.json";
    private readonly EEPROMManager _eepromManager;

    public List<Pad> Pads { get; private set; } = new();


    public PadManager(SerialManager serialManager)
    {
        _eepromManager = new EEPROMManager(serialManager);
    }

    public void LoadPads()
    {
        if (!File.Exists(_configPath))
        {
            CreateDefaultPads();
            SavePads(Pads);
            return;
        }

        try
        {
            string json = File.ReadAllText(_configPath);
            var pads = JsonSerializer.Deserialize<List<Pad>>(json);
            Pads = pads ?? new List<Pad>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"[PadManager] Erro ao carregar JSON: {ex.Message}");
            CreateDefaultPads();
            SavePads(Pads);
        }
    }

    public void SavePads(List<Pad> Pads)
    {
        try
        {
            string json = JsonSerializer.Serialize(Pads, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"[PadManager] Erro ao salvar JSON: {ex.Message}");
        }
    }

    private void CreateDefaultPads()
    {
        Pads.Clear();
        for (int i = 0; i < 15; i++)
        {
            Pads.Add(new Pad
            {

                Type = 0,
                Name = $"Pad {i + 1}", // assumindo que a propriedade é Name, não PadName
                Note = 0,
                Threshold = 0,
                ScanTime = 0,
                MaskTime = 0,
                Retrigger = 0,
                Curve = 0,
                CurveForm = 0,
                Xtalk = 0,
                XtalkGroup = 0,
                Channel = 1,
                Gain = 0
            });
        }
    }

    // Se quiser externar o path
    public string GetConfigPath() => _configPath;


}
