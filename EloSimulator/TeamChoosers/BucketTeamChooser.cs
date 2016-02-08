using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Choose teams based upon placement into Elo buckets
    /// </summary>
    public class BucketTeamChooser : ITeamChooser
    {
        /// <summary>
        /// 
        /// </summary>
        public int EloRange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IPlayerFinder Finder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="finder"></param>
        public BucketTeamChooser( IPlayerFinder finder )
        {
            if ( finder != null )
                Finder = finder;
            else
            {
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
            return chooseTeams( players, Finder, seed, playersPerTeam, EloRange );
        }

        /// <summary>
        /// Choose teams based upon placement into Elo buckets
        /// </summary>
        /// <param name="players"></param>
        /// <param name="finder"></param>
        /// <param name="seed"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="eloRange"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeams( List<Player> players, IPlayerFinder finder, Player seed, int playersPerTeam, int eloRange )
        {
            //Find possible players
            List<Player> possible = finder.FindPlayers( players, seed.GetElo(), eloRange );

            //Add seed to possible players if not already present
            if ( !possible.Contains( seed ) )
                possible.Add( seed );

            //Check if there are enough players
            if ( possible.Count >= 2 * playersPerTeam )
            {
                //Remove seed from list of possible players
                possible.Remove( seed );

                //Bucket players into teams
                return bucketPlayers( possible, playersPerTeam, seed );
            }

            //Return a tuple of EmptyTeams since we don't have enough players for two Teams
            return new Tuple<Team, Team>( new EmptyTeam(), new EmptyTeam() );
        }

        /// <summary>
        /// Divide players into two teams with even players on one team and odd players on the other
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private Tuple<Team, Team> bucketPlayers( List<Player> players, int playersPerTeam, Player seed )
        {
            //Create teams
            Team a = new Team();
            Team b = new Team();

            //Add seed to first Team
            a.Players.Add( seed );

            //Divide players between Teams
            for ( int count = 1; count < 2 * playersPerTeam; count++ )
            {
                if ( count % 2 == 0 )
                    a.Players.Add( players[count - 1] );
                else
                    b.Players.Add( players[count - 1] );
            }

            //Return Teams
            return new Tuple<Team, Team>( a, b );
        }
    }
}
