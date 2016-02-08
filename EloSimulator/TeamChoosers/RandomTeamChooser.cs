using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Choose teams through random assignment
    /// </summary>
    public class RandomTeamChooser : ITeamChooser
    {
        /// <summary>
        /// 
        /// </summary>
        public Random Random { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        public RandomTeamChooser( Random r )
        {
            if ( r != null )
                Random = r;
            else
                Random = new Random();
        }

        /// <summary>
        /// Divide players randomly into two Teams
        /// </summary>
        /// <param name="players"></param>
        /// <param name="seed">Unused</param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        public Tuple<Team, Team> ChooseTeams( List<Player> players, Player seed, int playersPerTeam )
        {
            return chooseTeams( players, seed, playersPerTeam, Random );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="seed"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeams( List<Player> players, Player seed, int playersPerTeam, Random r )
        {
            if ( players.Count >= 2 * playersPerTeam )
            {
                List<Player> t = players.RandomOrdering( r, 2 * playersPerTeam ).ToList();
                Team a = new Team();
                Team b = new Team();

                for ( int count = 0; count < 2 * playersPerTeam; count++ )
                {
                    if ( count < playersPerTeam )
                    {
                        a.Players.Add( t[count] );
                    }
                    else
                    {
                        b.Players.Add( t[count] );
                    }
                }

                return new Tuple<Team, Team>( a, b );
            }

            //Return a tuple of EmptyTeams because we never got enough players together
            return new Tuple<Team, Team>( new EmptyTeam(), new EmptyTeam() );
        }
    }
}
