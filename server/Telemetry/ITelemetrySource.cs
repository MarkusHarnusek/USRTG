namespace USRTG.Telemetry
{
    /// <summary>A game adapter. One instance per game (or per family of games sharing a transport).</summary>
    internal interface ITelemetrySource : IDisposable
    {
        string Name { get; }

        /// <summary>
        /// Returns the current full packet when the game is running (even while it sits in a menu,
        /// in which case GameState is Off), or null when the game is not running / no data is flowing.
        /// Must never throw for "game not available".
        /// </summary>
        Packet? TryRead();
    }
}
