namespace DyDrums.Helpers
{
    public static class DataGridValidationRules
    {
        // Limites numéricos por coluna
        public static Dictionary<string, (int Min, int Max)> NumericLimits = new()
    {
        { "Note", (0, 127) },
        { "Threshold", (0, 127) },
        { "Gain", (0, 127) },
        { "Curve", (0, 4) },
        { "CurveForm", (0, 127) },
        { "XtalkGroup", (0, 3) },
        { "Channel", (1, 16) }
    };

        // Combobox com texto e valor real que vai pro JSON/Arduino
        public class ComboOption
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString() => Text;
        }

        public static Dictionary<string, List<ComboOption>> ComboBoxValues = new()
    {
        { "Type", new List<ComboOption> {
            new ComboOption { Text = "Piezo", Value = 0 },
            new ComboOption { Text = "Switch", Value = 1 },
            new ComboOption { Text = "HHC", Value = 2 },
            new ComboOption { Text = "Disable", Value = 3 }
        }},
        { "Curve", new List<ComboOption> {
            new ComboOption { Text = "Linear", Value = 0 },
            new ComboOption { Text = "Exponencial", Value = 1 },
            new ComboOption { Text = "Logarítmica", Value = 2 },
            new ComboOption { Text = "Sigmoide", Value = 3 },
            new ComboOption { Text = "Flat", Value = 4 }
        }}
    };
    }

}
