using System;
using System.Collections.Generic;
using System.Linq;

namespace USRTG.AC
{
    public class ABSMapping
    {
        private readonly Dictionary<string, SortedSet<float>> carCurves = new();
        private readonly float epsilon = 0.001f;

        public void UpdateCurve(string carModel, float abs)
        {
            if (abs <= 0)
            {
                return; 
            }

            if (!carCurves.TryGetValue(carModel, out var curve))
            {
                curve = new SortedSet<float>();
                carCurves[carModel] = curve;
            }

            if (!curve.Any(k => Math.Abs(k - abs) < epsilon))
            {
                curve.Add(abs);
            }
        }

        public int GetABSLevel(string carModel, float abs)
        {
            if (Math.Abs(abs) < epsilon)
            {
                return 0; 
            }

            if (!carCurves.TryGetValue(carModel, out var curve) || curve.Count == 0)
            {
                return -1; 
            }

            float? matchedAbs = curve.FirstOrDefault(k => Math.Abs(k - abs) < epsilon);
            if (!matchedAbs.HasValue)
            {
                return -1;
            }

            var sortedAscending = curve.OrderBy(k => k).ToList();

            for (int i = 0; i < sortedAscending.Count; i++)
            {
                if (Math.Abs(sortedAscending[i] - abs) < epsilon)
                {
                    return i + 1;
                }
            }

            return -1; 
        }

        public void Clear() => carCurves.Clear();

        // Debug method to get all discovered levels
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