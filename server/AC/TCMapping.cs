namespace USRTG.AC
{
    public class TCMapping
    {
        private List<float> tcCurve = new List<float>();
        private string lastCarModel = string.Empty;
        private string acInstallPath = "";

        public TCMapping(string installPath)
        {
            acInstallPath = installPath;
        }

        // Call this when car changes (check SPageFileStatic.carModel)
        public void LoadCurveForCar(string carModel)
        {
            if (carModel == lastCarModel) return;

            string iniPath = Path.Combine(acInstallPath, "content", "cars", carModel, "data", "electronics.ini");
            if (!File.Exists(iniPath))
            {
                // Handle error: no file, assume no multi-level TC or fallback to tc * 10 or something
                tcCurve.Clear();
                lastCarModel = string.Empty;
                return;
            }

            bool inTCSection = false;
            tcCurve.Clear();

            foreach (string line in File.ReadLines(iniPath))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("[TRACTION_CONTROL]"))
                {
                    inTCSection = true;
                    continue;
                }
                if (inTCSection && trimmed.StartsWith("["))
                {
                    inTCSection = false; // End of section
                    break;
                }
                if (inTCSection && trimmed.StartsWith("CURVE="))
                {
                    string curveStr = trimmed.Substring("CURVE=".Length).Trim();
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
                    break; // Found CURVE, no need to continue
                }
            }

            lastCarModel = carModel;
        }

        // Call this after reading tc from shared memory
        public int GetTCLevel(float tc)
        {
            if (tc == 0) return 0; // Off

            if (tcCurve.Count == 0)
            {
                return -1; // Error, no curve loaded
            }

            for (int i = 0; i < tcCurve.Count; i++)
            {
                if (Math.Abs(tcCurve[i] - tc) < 0.001f)
                {
                    return i + 1; // 1-based level
                }
            }

            return -1; // No match, possible error
        }
    }
}