using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USRTG
{
    public class Mapping
    {
        public int MapGameState(int state, Enums.Game game)
        {
            switch (game)
            {
                case Enums.Game.AssettoCorsa:
                    return state; // Direct mapping for Assetto Corsa
                default:
                    throw new NotImplementedException();
            }
        }

        public int MapSessionType(int sessionType, Enums.Game game)
        {
            switch (game)
            {
                case Enums.Game.AssettoCorsa:
                    return sessionType; // Direct mapping for Assetto Corsa
                default:
                    throw new NotImplementedException();
            }
        }

        public int MapFlagType(int flagType, Enums.Game game)
        {
            switch (game)
            {
                case Enums.Game.AssettoCorsa:
                    var flagMapping = new Dictionary<int, int>
                    {
                        { 0, 0 }, // No Flag
                        { 1, 7 }, // Blue Flag
                        { 2, 2 }, // Yellow Flag
                        { 3, 9 }, // Black Flag
                        { 4, 5 }, // White Flag
                        { 5, 6 }, // Checkered Flag
                        { 6, 9 }  // Penalty Flag
                    };

                    if (flagMapping.TryGetValue(flagType, out int mappedFlag))
                    {
                        return mappedFlag;
                    }
                    else
                    {
                        Console.WriteLine("Unknown flag type: " + flagType);
                        return 0; // Default to No Flag
                    }

                default:
                    throw new NotImplementedException();
            }
        }

    }
}
