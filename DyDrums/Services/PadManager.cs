using System.Text.Json;
using DyDrums.Models;
using DyDrums.Services;

public class PadManager
{
    public event Action<List<Pad>>? PadsUpdated;
    private readonly string _configPath = "pads.json";
    private readonly EEPROMManager _eepromManager;

    public List<Pad> Pads { get; private set; } = new();
    private List<byte[]> _receivedMessages = new();
    private bool _alreadyProcessed = false;

    public PadManager(SerialManager serialManager)
    {
        _eepromManager = new EEPROMManager(serialManager);
    }

    public void LoadPads()
    {
        if (!File.Exists(_configPath))
        {
            CreateDefaultPads();
            SavePads();
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
            //Debug.WriteLine($"[PadManager] Erro ao carregar JSON: {ex.Message}");
            CreateDefaultPads();
            SavePads();
        }
    }

    public void SavePads()
    {
        try
        {
            MessageBox.Show(Pads.Count().ToString());
            string json = JsonSerializer.Serialize(Pads, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            //Debug.WriteLine($"[PadManager] Erro ao salvar JSON: {ex.Message}");
        }
    }

    private void CreateDefaultPads()
    {
        Pads.Clear();
        for (int i = 0; i < 15; i++)
        {
            Pads.Add(new Pad
            {
                Pin = i,
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



    // Você pode adicionar esse método se quiser externar o path
    public string GetConfigPath() => _configPath;


}
