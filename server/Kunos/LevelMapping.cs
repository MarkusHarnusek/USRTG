namespace USRTG.Kunos
{
    /// <summary>
    /// Assetto Corsa only exposes the raw traction-control / ABS value of the current setting, not the
    /// level index the driver sees. This class learns the distinct values per car as the driver cycles
    /// through the levels and reports the 1-based index of the current value in the sorted set.
    ///
    /// Limitations (inherent to the approach):
    ///  - A level can only be reported once it has been seen, so indices can shift while levels are
    ///    still being discovered (a newly seen lower value renumbers the ones above it).
    ///  - -1 is reported for a value that has not been classified yet, 0 means the aid is off.
    ///
    /// This replaces the two identical TCMapping/ABSMapping classes.
    /// </summary>
    public sealed class LevelMapping
    {
        private const float Epsilon = 0.001f;
        /// <summary>Guards against unbounded growth if a car reports a continuously varying value.</summary>
        private const int MaxLevelsPerCar = 32;

        private readonly Dictionary<string, List<float>> _carCurves = new(StringComparer.Ordinal);

        /// <summary>Records <paramref name="value"/> as a known level for <paramref name="carModel"/> and returns its level.</summary>
        public int Update(string? carModel, float value)
        {
            if (value <= Epsilon)
            {
                return 0;
            }

            carModel ??= "";
            if (!_carCurves.TryGetValue(carModel, out var curve))
            {
                curve = new List<float>();
                _carCurves[carModel] = curve;
            }

            int index = IndexOf(curve, value);
            if (index >= 0)
            {
                return index + 1;
            }

            if (curve.Count >= MaxLevelsPerCar)
            {
                return -1;
            }

            int insertAt = curve.FindIndex(k => k > value);
            if (insertAt < 0)
            {
                insertAt = curve.Count;
            }
            curve.Insert(insertAt, value);
            return insertAt + 1;
        }

        public int GetLevel(string? carModel, float value)
        {
            if (value <= Epsilon)
            {
                return 0;
            }

            if (!_carCurves.TryGetValue(carModel ?? "", out var curve))
            {
                return -1;
            }

            int index = IndexOf(curve, value);
            return index >= 0 ? index + 1 : -1;
        }

        public void Clear() => _carCurves.Clear();

        public string GetDiscoveredLevels(string? carModel)
        {
            if (!_carCurves.TryGetValue(carModel ?? "", out var curve))
            {
                return "None";
            }

            return string.Join(", ", curve.Select((k, index) => $"{(int)(k * 100)} ({index + 1})"));
        }

        private static int IndexOf(List<float> curve, float value)
        {
            for (int i = 0; i < curve.Count; i++)
            {
                if (Math.Abs(curve[i] - value) < Epsilon)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
