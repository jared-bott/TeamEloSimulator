using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Find players within an Elo range
    /// </summary>
    public class SimplePlayerFinder : IPlayerFinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="maxElo"></param>
        /// <param name="eloRange"></param>
        /// <returns></returns>
        public List<Player> FindPlayers( List<Player> players, double maxElo, int eloRange )
        {
            List<Player> possible = players.FindAll( x => x.GetElo() <= maxElo && x.GetElo() >= maxElo - eloRange );

            return possible;
        }
    }
}
