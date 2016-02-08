using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Choose teams based upon a seed player, with expanding Elo range when there aren't enough players in Elo range
    /// </summary>
    public class SeededTeamChooser : ITeamChooser
    {
        /// <summary>
        /// Maximum variance in Elo between players on a Team
        /// </summary>
        public int EloRange { get; set; }
        /// <summary>
        /// Maximum Elo possible
        /// </summary>
        public int MaxElo { get; set; }
        /// <summary>
        /// Minimum Elo possible
        /// </summary>
        public int MinElo { get; set; }

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
        public SeededTeamChooser( ITeamDivider divider, IPlayerFinder finder )
        {
            MaxElo = 2800;
            MinElo = 0;

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
            return chooseTeams( players, Divider, Finder, seed, playersPerTeam, EloRange, MaxElo, MinElo );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="divider"></param>
        /// <param name="finder"></param>
        /// <param name="seed"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="eloRange"></param>
        /// <param name="maxElo"></param>
        /// <param name="minElo"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeams( List<Player> players, ITeamDivider divider, IPlayerFinder finder, Player seed, int playersPerTeam, int eloRange, int maxElo, int minElo )
        {
            int max = 0;
            int min = 0;

            //Check if the range of Elos to check hits the max
            if ( seed.GetElo() + eloRange / 2 > maxElo )
            {
                //Set max to maxElo
                max = maxElo;
                //Set min
                min = seed.GetElo() - (eloRange / 2) - (maxElo - seed.GetElo());
            }
            else
            {
                //Check if the range of Elos to check hits the min
                if ( seed.GetElo() - eloRange / 2 < minElo )
                {
                    //Set min to minElo
                    min = minElo;
                    //Set max
                    max = seed.GetElo() + (eloRange / 2) + (seed.GetElo() - minElo);
                }
                else
                {
                    //Set max
                    max = seed.GetElo() + eloRange / 2;
                    //Set min
                    min = seed.GetElo() - eloRange / 2;
                }
            }

            //Get possible players
            List<Player> possible = finder.FindPlayers( players, max, max - min );

            //Add seed to list of possible players
            if ( !possible.Contains( seed ) )
            {
                possible.Add( seed );
            }

            //Check if there are enough players to make a match
            if ( possible.Count >= 2 * playersPerTeam )
            {
                //Enough players
                possible.Remove( seed );

                //Divide the players into two teams
                return divider.DividePlayers( possible, playersPerTeam, seed );
            }
            else
            {
                //Not enough players, expand the Elo range
                if ( eloRange < maxElo - minElo )
                    return chooseTeams( players, divider, finder, seed, playersPerTeam, eloRange * 2, maxElo, minElo );
            }

            //Return a tuple of EmptyTeams because we never got enough players together
            return new Tuple<Team, Team>( new EmptyTeam(), new EmptyTeam() );
        }
    }
}
