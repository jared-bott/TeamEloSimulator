using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Divide players into two Teams based upon a seed player
    /// </summary>
    public class SimpleTeamChooser : ITeamChooser
    {
        /// <summary>
        /// 
        /// </summary>
        public int EloRange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ITeamDivider Divider { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IPlayerFinder Finder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="divider"></param>
        /// <param name="finder"></param>
        public SimpleTeamChooser( ITeamDivider divider, IPlayerFinder finder )
        {
            if ( divider != null )
                Divider = divider;
            else
            {
                //Create a default team divider
                Divider = new SimpleTeamDivider();
            }

            if ( finder != null )
                Finder = finder;
            else
            {
                //Create a default player finder
                Finder = new SimplePlayerFinder();
            }
        }

        /// <summary>
        /// Divide players into two Teams
        /// </summary>
        /// <param name="players"></param>
        /// <param name="seed"></param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        public Tuple<Team, Team> ChooseTeams( List<Player> players, Player seed, int playersPerTeam )
        {
            return chooseTeams( players, Finder, Divider, seed, playersPerTeam, EloRange );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="finder"></param>
        /// <param name="divider"></param>
        /// <param name="seed"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="eloRange"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeams( List<Player> players, IPlayerFinder finder, ITeamDivider divider, Player seed, int playersPerTeam, int eloRange )
        {
            List<Player> possible = finder.FindPlayers( players, seed.GetElo(), eloRange );

            if ( !possible.Contains( seed ) )
            {
                possible.Add( seed );
            }

            if ( possible.Count >= 2 * playersPerTeam )
            {
                possible.Remove( seed );

                return divider.DividePlayers( possible, playersPerTeam, seed );
            }

            //Return a tuple of EmptyTeams because we never got enough players together
            return new Tuple<Team, Team>( new EmptyTeam(), new EmptyTeam() );
        }
    }
}
