using Microsoft.Win32;
using System.Text.RegularExpressions;

#pragma warning disable CA1416
namespace USRTG.AC
{
    public class TCMapping
    {
        private List<float> tcCurve = new List<float>();
        private string lastCarModel = string.Empty;
        private readonly string acInstallPath;
        private const int AC_APPID = 244210;

        public TCMapping()
        {
            acInstallPath = FindAssettoCorsaPath();
            if (string.IsNullOrEmpty(acInstallPath))
            {
                acInstallPath = @"C:\Program Files (x86)\Steam\steamapps\common\assettocorsa";
                if (!Directory.Exists(acInstallPath))
                {
                    acInstallPath = string.Empty;
                }
            }
        }

        private static string FindAssettoCorsaPath()
        {
            // Step 1: Find SteamPath from registry (HKCU preferred)
            string[] regSubKeys = { @"Software\Valve\Steam", @"Software\Wow6432Node\Valve\Steam" };
            string steamPath = string.Empty;
            foreach (string subKeyName in regSubKeys)
            {
                using (RegistryKey? keyCU = Registry.CurrentUser.OpenSubKey(subKeyName))
                {
                    if (keyCU?.GetValue("SteamPath") is string pathCU)
                    {
                        steamPath = pathCU.Replace("/", "\\").TrimEnd('\\');
                        break;
                    }
                }
                if (steamPath != null) break;

                using (RegistryKey? keyLM = Registry.LocalMachine.OpenSubKey(subKeyName))
                {
                    if (keyLM?.GetValue("SteamPath") is string pathLM)
                    {
                        steamPath = pathLM.Replace("/", "\\").TrimEnd('\\');
                        break;
                    }
                }
                if (steamPath != null) break;
            }

            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                return string.Empty;
            }

            // Step 2: Get all steamapps/common directories from libraryfolders.vdf
            List<string> commonDirs = new List<string>();
            string mainCommon = Path.Combine(steamPath, "steamapps", "common");
            if (Directory.Exists(mainCommon)) commonDirs.Add(mainCommon);

            string libVdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libVdfPath))
            {
                string[] lines = File.ReadAllLines(libVdfPath);
                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, @"""path""\s+""([^""]+)""");
                    if (match.Success)
                    {
                        string libPath = match.Groups[1].Value.Replace("\\\\", "\\");
                        string libCommon = Path.Combine(libPath, "steamapps", "common");
                        if (Directory.Exists(libCommon)) commonDirs.Add(libCommon);
                    }
                }
            }

            // Step 3: For each common dir, check appmanifest_244210.acf in parent steamapps/
            foreach (string commonDir in commonDirs)
            {
                string steamappsDir = Path.GetDirectoryName(commonDir) ?? string.Empty;
                string acfPath = Path.Combine(steamappsDir, $"appmanifest_{AC_APPID}.acf");
                if (!File.Exists(acfPath)) continue;

                string[] acfLines = File.ReadAllLines(acfPath);
                foreach (string line in acfLines)
                {
                    Match match = Regex.Match(line, @"""installdir""\s+""([^""]+)""");
                    if (match.Success)
                    {
                        string installdir = match.Groups[1].Value;
                        string candidatePath = Path.Combine(commonDir, installdir);
                        if (Directory.Exists(candidatePath) &&
                            File.Exists(Path.Combine(candidatePath, "assetto_corsa.exe")))
                        {
                            return candidatePath;
                        }
                    }
                }
            }

            return string.Empty;
        }

        // Call when SPageFileStatic updates (carModel changes)
        public void LoadCurveForCar(string carModel)
        {
            if (string.IsNullOrEmpty(acInstallPath) || carModel == lastCarModel)
            {
                return;
            }

            Console.WriteLine($"Loading TC curve for car model: {carModel}");

            string iniPath = Path.Combine(acInstallPath, "content", "cars", carModel, "data", "electronics.ini");
            if (!File.Exists(iniPath))
            {
                tcCurve.Clear();
                lastCarModel = string.Empty;
                return;
            }

            bool inTCSection = false;
            tcCurve.Clear();

            foreach (string line in File.ReadLines(iniPath))
            {
                string trimmed = line.Trim();
                if (trimmed.Equals("[TRACTION_CONTROL]", StringComparison.OrdinalIgnoreCase))
                {
                    inTCSection = true;
                    continue;
                }
                if (inTCSection && trimmed.StartsWith("["))
                {
                    inTCSection = false;
                    break;
                }
                if (inTCSection && trimmed.StartsWith("CURVE=", StringComparison.OrdinalIgnoreCase))
                {
                    string curveStr = trimmed.Substring(6).Trim();
                    if (!string.IsNullOrEmpty(curveStr))
                    {
                        string[] values = curveStr.Split(',');
                        foreach (string val in values)
                        {
                            if (float.TryParse(val.Trim(), out float slip))
                            {
                                tcCurve.Add(slip);
                            }
                        }
                    }
                    break;
                }
            }

            lastCarModel = carModel;
        }

        // Returns 0 for off, 1-N for levels, -1 for error/no match
        public int GetTCLevel(float tc)
        {
            if (Math.Abs(tc) < 0.001f) return 0; // Off

            if (tcCurve.Count == 0) return -1;

            for (int i = 0; i < tcCurve.Count; i++)
            {
                if (Math.Abs(tcCurve[i] - tc) < 0.001f)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        public bool HasValidPath => !string.IsNullOrEmpty(acInstallPath) && Directory.Exists(acInstallPath);
        public string InstallPath => acInstallPath ?? "Not found";
    }
}
