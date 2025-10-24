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


    }
}
