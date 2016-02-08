using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Divide players into two teams based upon a seed player
    /// </summary>
    public class SimpleTeamDivider : ITeamDivider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Tuple<Team, Team> DividePlayers( List<Player> players, int playersPerTeam, Player seed )
        {
            //if ( players.Count > 2 * PLAYERS_PER_TEAM )
            //players.Sort( ( p1, p2 ) => p2.GetElo().CompareTo( p1.GetElo() ) );
            //players.Sort( ( p1, p2 ) => Math.Abs( seed.GetElo() - p2.GetElo() ) );
            int seedElo = seed.GetElo();
            players.Sort( ( p1, p2 ) => Math.Abs( seedElo - p1.GetElo() ).CompareTo( Math.Abs( seedElo - p2.GetElo() ) ) );
            //players.OrderByDescending( p => Math.Abs( seedElo - p.GetElo() ) );

            Team a = new Team();
            Team b = new Team();

            a.Players.Add( seed );

            for ( int count = 1; count < 2 * playersPerTeam; count++ )
            {
                if ( count % 2 == 0 )
                    a.Players.Add( players[count - 1] );
                else
                    b.Players.Add( players[count - 1] );
            }

            return new Tuple<Team, Team>( a, b );
        }
    }
}
