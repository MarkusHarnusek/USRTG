namespace USRTG
{
    /// <summary>Small conversion helpers shared by the game handlers.</summary>
    internal static class Units
    {
        public static float Percent(float value, float max) =>
            max > 0 ? Math.Clamp(value / max * 100f, 0f, 100f) : 0f;

        /// <summary>
        /// Kunos accG is [x, y, z] in the car's frame where x is lateral, y vertical and z longitudinal.
        /// Reordered to the packet convention [lateral, longitudinal, vertical].
        /// </summary>
        public static float[] KunosAccG(float[]? accG) =>
            accG is { Length: >= 3 } ? [accG[0], accG[2], accG[1]] : new float[3];

        /// <summary>
        /// F1 2020 wheel arrays are [RL, RR, FL, FR]; the packet convention is [FL, FR, RL, RR].
        /// </summary>
        public static float[] F1Wheels(float[] rlRrFlFr) =>
            [rlRrFlFr[2], rlRrFlFr[3], rlRrFlFr[0], rlRrFlFr[1]];

        public static float[] F1Wheels(ushort[] rlRrFlFr) =>
            [rlRrFlFr[2], rlRrFlFr[3], rlRrFlFr[0], rlRrFlFr[1]];

        public static float[] F1Wheels(byte[] rlRrFlFr) =>
            [rlRrFlFr[2], rlRrFlFr[3], rlRrFlFr[0], rlRrFlFr[1]];

        public static string JoinName(string? first, string? last)
        {
            first = first?.Trim() ?? "";
            last = last?.Trim() ?? "";
            return last.Length == 0 ? first : first.Length == 0 ? last : first + " " + last;
        }

        /// <summary>Finds the player's coordinates in the ACC multi-car arrays.</summary>
        public static float[]? PlayerCoordinates(float[]? coordinates, int[]? carIds, int playerCarId, int activeCars)
        {
            if (coordinates == null || carIds == null)
            {
                return null;
            }

            int count = Math.Min(Math.Max(activeCars, 0), carIds.Length);
            for (int i = 0; i < count; i++)
            {
                if (carIds[i] == playerCarId && (i * 3 + 2) < coordinates.Length)
                {
                    return [coordinates[i * 3], coordinates[i * 3 + 1], coordinates[i * 3 + 2]];
                }
            }
            return null;
        }

        /// <summary>Seconds (float) to whole milliseconds.</summary>
        public static int SecondsToMs(float seconds) =>
            seconds <= 0 || float.IsNaN(seconds) ? 0 : (int)MathF.Round(seconds * 1000f);
    }
}
