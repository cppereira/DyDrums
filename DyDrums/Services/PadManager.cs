using System.Diagnostics;
using System.Text.Json;
using DyDrums.Models;
using DyDrums.Services;

public class PadManager
{
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
            Debug.WriteLine($"[PadManager] Erro ao carregar JSON: {ex.Message}");
            CreateDefaultPads();
            SavePads();
        }
    }

    public void SavePads()
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
            Debug.WriteLine($"[PadManager] Erro ao salvar JSON: {ex.Message}");
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
                PadName = $"Pad {i + 1}", // assumindo que a propriedade é Name, não PadName
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

    public void ResetSysexProcessing()
    {
        _alreadyProcessed = false;
    }

    public void ProcessSysex(List<byte[]> messages)
    {
        _receivedMessages.AddRange(messages);

        if (_alreadyProcessed || !_receivedMessages.Any(IsEndMessage))
            return;

        _alreadyProcessed = true;

        var pads = _eepromManager.ParseSysex(_receivedMessages)
            .Where(p => p != null && p.Pin >= 0 && p.Pin < 15)
            .ToList();

        var existingPads = LoadFromDisk();

        foreach (var pad in pads)
        {
            var match = existingPads.FirstOrDefault(p => p.Pin == pad.Pin);
            if (match != null)
            {
                pad.PadName = match.PadName;
            }
        }

        LoadConfigs(pads);
        SavePads();
        _receivedMessages.Clear();
    }

    private List<Pad> LoadFromDisk()
    {
        if (!File.Exists(_configPath)) return new List<Pad>();
        string json = File.ReadAllText(_configPath);
        return JsonSerializer.Deserialize<List<Pad>>(json) ?? new List<Pad>();
    }

    public void LoadConfigs(List<Pad> newPads)
    {
        Pads = newPads;
    }

    // Você pode adicionar esse método se quiser externar o path
    public string GetConfigPath() => _configPath;

    // Stub provisório pra compilar — você pode implementar sua lógica
    private bool IsEndMessage(byte[] message)
    {
        return message.Length >= 2 && message[0] == 0xF0 && message[^1] == 0xF7 &&
               message.Length >= 7 && message[1] == 0x77; // ou qualquer lógica de término
    }
}
