namespace ShadowCheat.Other
{
    public static class KeybindNameManager
    {
        public static string ConvertToRegularKey(string key)
        {
            return key switch
            {
                "LButton" => "LMB",
                "RButton" => "RMB",
                "MButton" => "MMB",
                "XButton1" => "MB4",
                "XButton2" => "MB5",
                "LShiftKey" => "LShift",
                "RShiftKey" => "RShift",
                "LControlKey" => "LCtrl",
                "RControlKey" => "RCtrl",
                "LMenu" => "LAlt",
                "RMenu" => "RAlt",
                "Return" => "Enter",
                "Space" => "Space",
                "Capital" => "Caps",
                "Prior" => "PgUp",
                "Next" => "PgDn",
                "Snapshot" => "PrtSc",
                "OemPeriod" => ".",
                "Oemcomma" => ",",
                "OemMinus" => "-",
                "Oemplus" => "+",
                "OemQuestion" => "/",
                "Oemtilde" => "~",
                "OemOpenBrackets" => "[",
                "OemCloseBrackets" => "]",
                "OemQuotes" => "'",
                "OemSemicolon" => ";",
                "OemBackslash" => "\\",
                "None" => "None",
                _ => key.Length == 2 && key.StartsWith("D") && char.IsDigit(key[1])
                    ? key[1].ToString()
                    : key.Length == 1 ? key : key.Replace("NumPad", "Num").Replace("Oem", "")
            };
        }
    }
}
