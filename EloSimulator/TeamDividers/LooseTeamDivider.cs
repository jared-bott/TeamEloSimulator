using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Divide players into two teams with some players on each team having guaranteed close Elo scores, the rest assigned based upon total team Elo
    /// </summary>
    public class LooseTeamDivider : ITeamDivider
    {
        /// <summary>
        /// Total number of players in a game that need to have close Elo scores
        /// </summary>
        public int NumClose { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private Random Random { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numClose"></param>
        public LooseTeamDivider( int numClose, Random r )
        {
            NumClose = numClose;

            if ( r != null )
                Random = r;
            else
                Random = new Random();
        }

        /// <summary>
        /// Divide players into teams
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public Tuple<Team, Team> DividePlayers( List<Player> players, int playersPerTeam, Player seed )
        {
            return dividePlayers( players, playersPerTeam, seed, NumClose, Random );
        }

        /// <summary>
        /// Divide players into two teams with some players on each team having guaranteed close Elo scores, the rest assigned based upon total team Elo
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <param name="numClose"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private Tuple<Team, Team> dividePlayers( List<Player> players, int playersPerTeam, Player seed, int numClose, Random r )
        {
            if ( numClose < 2 * playersPerTeam )
            {
                //Sort the players so that both teams get some of the top available players
                players.Sort( ( p1, p2 ) => p2.GetElo().CompareTo( p1.GetElo() ) );

                //Create two Teams
                Team a = new Team();
                Team b = new Team();
                int startIndex = 0;
                int totalPlayersAssigned = 0;

                //Check if all of the players must be close in Elo
                if ( numClose == 2 * playersPerTeam )
                {
                    //Add the seed player to one team
                    a.Players.Add( seed );
                    startIndex = 1;
                }

                //Add the number of players that need to be close to teams
                for ( int count = startIndex; count < numClose; count++ )
                {
                    if ( count % 2 == 0 )
                        a.Players.Add( players[count - 1] );
                    else
                        b.Players.Add( players[count - 1] );
                }

                totalPlayersAssigned = numClose;

                int numLeft = 2 * playersPerTeam - numClose;

                if ( numLeft > 0 )
                {
                    //Add the seed player to one team
                    a.Players.Add( seed );
                    numLeft--;
                    totalPlayersAssigned++;
                }

                //Get as many players as are needed and randomize them
                players = players.GetRange( numClose, players.Count - totalPlayersAssigned ).RandomOrdering( r ).ToList();

                //Assign rest of players as needed
                for ( int count = 0; count < numLeft; count++ )
                {
                    //Don't reassign the seed
                    if ( players[count] != seed )
                    {
                        //Check if both teams need a player
                        if ( a.Players.Count < playersPerTeam && b.Players.Count < playersPerTeam )
                        {
                            //Calculate Elo differences
                            double diffWithA = ((double)(a.GetTeamElo() + players[count].GetElo())) / (double)(a.Players.Count + 1) - b.GetAverageElo();
                            double diffWithB = ((double)(b.GetTeamElo() + players[count].GetElo())) / (double)(b.Players.Count + 1) - a.GetAverageElo();

                            //Decide which team gets the player
                            if ( Math.Abs( diffWithA ) < Math.Abs( diffWithB ) )
                            {
                                a.Players.Add( players[count] );
                            }
                            else
                            {
                                b.Players.Add( players[count] );
                            }
                        }
                        //Assign to Team A
                        else if ( a.Players.Count < playersPerTeam )
                            a.Players.Add( players[count] );
                        //Assign to Team B
                        else if ( b.Players.Count < playersPerTeam )
                            b.Players.Add( players[count] );
                    }
                }

                return new Tuple<Team, Team>( a, b );
            }

            //Return a game with EmptyTeams because there weren't enough players
            return new Tuple<Team, Team>( new EmptyTeam(), new EmptyTeam() );
        }
    }
}
