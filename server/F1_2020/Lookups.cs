using System.Collections.Frozen;

namespace USRTG.F1_2020
{
    /// <summary>Appendix tables of the F1 2020 UDP specification.</summary>
    internal static class Lookups
    {
        public static readonly FrozenDictionary<int, string> Tracks = new Dictionary<int, string>
        {
            { 0, "Melbourne" }, { 1, "Paul Ricard" }, { 2, "Shanghai" }, { 3, "Sakhir (Bahrain)" }, { 4, "Catalunya" },
            { 5, "Monaco" }, { 6, "Montreal" }, { 7, "Silverstone" }, { 8, "Hockenheim" }, { 9, "Hungaroring" },
            { 10, "Spa" }, { 11, "Monza" }, { 12, "Singapore" }, { 13, "Suzuka" }, { 14, "Abu Dhabi" },
            { 15, "Texas" }, { 16, "Brazil" }, { 17, "Austria" }, { 18, "Sochi" }, { 19, "Mexico" },
            { 20, "Baku (Azerbaijan)" }, { 21, "Sakhir Short" }, { 22, "Silverstone Short" }, { 23, "Texas Short" },
            { 24, "Suzuka Short" }, { 25, "Hanoi" }, { 26, "Zandvoort" }
        }.ToFrozenDictionary();

        public static readonly FrozenDictionary<int, string> Teams = new Dictionary<int, string>
        {
            { 0, "Mercedes" }, { 1, "Ferrari" }, { 2, "Red Bull Racing" }, { 3, "Williams" }, { 4, "Racing Point" },
            { 5, "Renault" }, { 6, "AlphaTauri" }, { 7, "Haas" }, { 8, "McLaren" }, { 9, "Alfa Romeo" },
            { 10, "McLaren 1988" }, { 11, "McLaren 1991" }, { 12, "Williams 1992" }, { 13, "Ferrari 1995" },
            { 14, "Williams 1996" }, { 15, "McLaren 1998" }, { 16, "Ferrari 2002" }, { 17, "Ferrari 2004" },
            { 18, "Renault 2006" }, { 19, "Ferrari 2007" }, { 20, "McLaren 2008" }, { 21, "Red Bull 2010" },
            { 22, "Ferrari 1976" }, { 23, "ART Grand Prix" }, { 24, "Campos Vexatec Racing" }, { 25, "Carlin" },
            { 26, "Charouz Racing System" }, { 27, "DAMS" }, { 28, "Russian Time" }, { 29, "MP Motorsport" },
            { 30, "Pertamina" }, { 31, "McLaren 1990" }, { 32, "Trident" }, { 33, "BWT Arden" },
            { 34, "McLaren 1976" }, { 35, "Lotus 1972" }, { 36, "Ferrari 1979" }, { 37, "McLaren 1982" },
            { 38, "Williams 2003" }, { 39, "Brawn 2009" }, { 40, "Lotus 1978" }, { 41, "F1 Generic car" },
            { 42, "Art GP '19" }, { 43, "Campos '19" }, { 44, "Carlin '19" }, { 45, "Sauber Junior Charouz '19" },
            { 46, "Dams '19" }, { 47, "Uni-Virtuosi '19" }, { 48, "MP Motorsport '19" }, { 49, "Prema '19" },
            { 50, "Trident '19" }, { 51, "Arden '19" }, { 53, "Benetton 1994" }, { 54, "Benetton 1995" },
            { 55, "Ferrari 2000" }, { 56, "Jordan 1991" }, { 63, "Ferrari 1990" }, { 64, "McLaren 2010" },
            { 65, "Ferrari 2010" }, { 255, "My Team" }
        }.ToFrozenDictionary();

        /// <summary>m_actualTyreCompound.</summary>
        public static readonly FrozenDictionary<int, string> ActualCompounds = new Dictionary<int, string>
        {
            { 16, "C5" }, { 17, "C4" }, { 18, "C3" }, { 19, "C2" }, { 20, "C1" }, { 7, "Intermediate" }, { 8, "Wet" },
            { 9, "Classic Dry" }, { 10, "Classic Wet" },
            { 11, "F2 Super Soft" }, { 12, "F2 Soft" }, { 13, "F2 Medium" }, { 14, "F2 Hard" }, { 15, "F2 Wet" }
        }.ToFrozenDictionary();

        /// <summary>m_tyreVisualCompound.</summary>
        public static readonly FrozenDictionary<int, string> VisualCompounds = new Dictionary<int, string>
        {
            { 16, "Soft" }, { 17, "Medium" }, { 18, "Hard" }, { 7, "Intermediate" }, { 8, "Wet" },
            { 9, "Classic Dry" }, { 10, "Classic Wet" },
            { 19, "F2 Super Soft" }, { 20, "F2 Soft" }, { 21, "F2 Medium" }, { 22, "F2 Hard" }, { 15, "F2 Wet" }
        }.ToFrozenDictionary();

        public static string Track(int id) => Tracks.TryGetValue(id, out var name) ? name : $"Track {id}";
        public static string Team(int id) => Teams.TryGetValue(id, out var name) ? name : $"Team {id}";
        public static string ActualCompound(int id) => ActualCompounds.TryGetValue(id, out var name) ? name : $"Compound {id}";
        public static string VisualCompound(int id) => VisualCompounds.TryGetValue(id, out var name) ? name : $"Compound {id}";
    }
}
