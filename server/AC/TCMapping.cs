using System;
using System.Collections.Generic;
using System.Linq;

namespace USRTG.AC
{
    public class TCMapping
    {
        private readonly Dictionary<string, SortedSet<float>> carCurves = new();
        private readonly float epsilon = 0.001f;

        public void UpdateCurve(string carModel, float tc)
        {
            if (tc <= 0)
            {
                return; 
            }

            if (!carCurves.TryGetValue(carModel, out var curve))
            {
                curve = new SortedSet<float>();
                carCurves[carModel] = curve;
            }

            if (!curve.Any(k => Math.Abs(k - tc) < epsilon))
            {
                curve.Add(tc);
            }
        }

        public int GetTCLevel(string carModel, float tc)
        {
            if (Math.Abs(tc) < epsilon)
            {
                return 0; 
            }

            if (!carCurves.TryGetValue(carModel, out var curve) || curve.Count == 0)
            {
                return -1; 
            }

            float? matchedTc = curve.FirstOrDefault(k => Math.Abs(k - tc) < epsilon);
            if (!matchedTc.HasValue)
            {
                return -1;
            }

            var sortedAscending = curve.OrderBy(k => k).ToList();

            // Find rank (level = index + 1)
            for (int i = 0; i < sortedAscending.Count; i++)
            {
                if (Math.Abs(sortedAscending[i] - tc) < epsilon)
                {
                    return i + 1;
                }
            }

            return -1; // Should not reach here
        }

        public void Clear() => carCurves.Clear();

        // Debug method to get all discoverd levels
        public string GetDiscoveredLevels(string carModel)
        {
            if (!carCurves.TryGetValue(carModel, out var curve))
            {
                return "None";
            }

            var sortedAscending = curve.OrderBy(k => k).ToList();
            return string.Join(", ", sortedAscending.Select((k, index) => $"{(int)(k * 100)} ({index + 1})"));
        }
    }
}