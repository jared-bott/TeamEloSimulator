/*
 *  Team Elo Simulator
    Copyright (C) 2015 Jared N. Bott

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace EloSimulator
{
    public class Simulator
    {
        private List<Player> _players = new List<Player>();
        private List<double> _eloDiffs = new List<double>();
        private int _numGamesPlayed = 0;

        private static Random r = new Random();
        private static readonly int PLAYERS_PER_TEAM = 12;
        private static readonly int K = 50;
        //private static readonly double ELO_UPDATE_VALUE = 0.2d;
        private static readonly int NUM_GAMES = 1000;
        private static readonly int NUM_PLAYERS = 2000;
        private static readonly bool USE_RANDOM_TEAMS = false;
        //private static readonly int NUM_SIMULTANEOUS_GAMES = 5;
        private static readonly int NUM_PLAYERS_ONLINE_AT_ONCE = 1000;
        private static readonly int MIN_ELO = 0;
        private static readonly int MAX_ELO = 2800;
        private static readonly double STD_DEV = 300;
        private static readonly bool SUM_ELOS = false;

        public ITeamChooser TeamChooser { get; set; }
        public IAllTeamsChooser AllTeamsChooser { get; set; }

        public Simulator( ITeamChooser teamChooser, IAllTeamsChooser allTeamsChooser )
        {
            TeamChooser = teamChooser;
            AllTeamsChooser = allTeamsChooser;

            initializePlayers( NUM_PLAYERS );
        }

        /// <summary>
        /// Run the number of games specified by NUM_GAMES
        /// </summary>
        public void Run()
        {
            Run( NUM_GAMES );
        }

        /// <summary>
        /// For numRounds rounds, divide players into teams, teams into games, and run all games
        /// </summary>
        /// <param name="numRounds">The number of rounds to run</param>
        public void Run( int numRounds )
        {
            //Iterate over the rounds
            for ( int count = 0; count < numRounds; count++ )
            {
                //Create some games
                List<Tuple<Team, Team>> games = AllTeamsChooser.ChooseAllTeams( _players, PLAYERS_PER_TEAM );
                
                //Play the games
                playGames( games );
            }

            //Save information about the games
            outputGameInfo( numRounds );
        }

        /// <summary>
        /// Save information about the games
        /// </summary>
        /// <param name="numGames"></param>
        private void outputGameInfo( int numGames )
        {
            _numGamesPlayed += numGames;

            //dumpElos( _players );
            dumpStats( _players );

            Dictionary<int, List<Player>> bins = binElos( _players, 25, 0, 2800 );

            String prefix = (USE_RANDOM_TEAMS) ? "RANDOM" : "";
            long ticks = DateTime.Now.Ticks;
            String path = "stats" + prefix + NUM_PLAYERS + "-" + _numGamesPlayed + "-" + ticks + ".csv";

            writeElos( path, _players );
            writeBins( "bins" + prefix + NUM_PLAYERS + "-" + _numGamesPlayed + "-" + ticks + ".csv", bins );
            writeEloDifferences( "eloDiffs" + prefix + NUM_PLAYERS + "-" + _numGamesPlayed + "-" + ticks + ".csv", _eloDiffs );

            Console.WriteLine( "Played " + _numGamesPlayed + " games" );
        }

        /// <summary>
        /// Run some games at once so that each player is only in one game at a time
        /// </summary>
        /// <param name="numGames"></param>
        public void RunOnly( int numGames )
        {
            List<Player> t = _players.RandomOrdering().ToList();
            List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();

            int count = 0;

            while ( count < numGames )
            {
                Tuple<Team, Team> game = TeamChooser.ChooseTeams( _players, t[count], PLAYERS_PER_TEAM );

                if ( game != null && !isEmptyGame( game ) )
                {
                    games.Add( game );

                    foreach ( Player p in game.Item1.Players )
                        t.Remove( p );

                    foreach ( Player p in game.Item2.Players )
                        t.Remove( p );
                }

                count++;
            }

            playGames( games );

            outputGameInfo( numGames );
        }

        /// <summary>
        /// Run multiple rounds of simultaneous games so that each player is only in one game at a time.
        /// </summary>
        /// <param name="numGames">The number of games to run simultaneously</param>
        /// <param name="numRounds">The number of rounds of simultaneous games</param>
        public void RunOnly( int numGames, int numRounds, ISeedChooser seedChooser )
        {
            for ( int i = 0; i < numRounds; i++ )
            {
                //Play a round of matches

                //Randomize players
                List<Player> t = _players.RandomOrdering().ToList();
                //List of games
                List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();
                //Get players that are online for this round
                List<Player> players = getSubset( t, NUM_PLAYERS_ONLINE_AT_ONCE );

                int count = 0;

                //Initialize seed chooser
                seedChooser.Initialize();

                //Create games
                while ( count < numGames )
                {
                    //Get a seed
                    Player seed = seedChooser.ChooseSeed( players );

                    if ( seed != null )
                    {
                        //Make a game
                        Tuple<Team, Team> game = TeamChooser.ChooseTeams( players, seed, PLAYERS_PER_TEAM );

                        //Check if the game is valid
                        if ( game != null && !isEmptyGame( game ) )
                        {
                            //Add to list of games to play
                            games.Add( game );

                            //Remove players so that they aren't in any games in this round
                            foreach ( Player p in game.Item1.Players )
                                players.Remove( p );

                            foreach ( Player p in game.Item2.Players )
                                players.Remove( p );
                        }
                        else
                        {
                            //Ignore a seed that couldn't make a valid game
                            seedChooser.IgnorePlayer( seed );
                        }
                    }

                    count++;
                }

                //Play out the games for this round
                playGames( games );
            }

            outputGameInfo( numGames * numRounds );
        }

        /// <summary>
        /// Create some players
        /// </summary>
        /// <param name="numPlayers">The number of players to create</param>
        private void initializePlayers( int numPlayers )
        {
            //Create some players
            for ( int count = 0; count < numPlayers; count++ )
            {
                Player p = new Player( count );
                //Assign a real Elo for the player to represent their actual skill
                p.RealElo = (int)Math.Round( sampleNormalDistribution() );

                _players.Add( p );
            }
        }

        /// <summary>
        /// Get a number from a normal distribution
        /// </summary>
        /// <returns></returns>
        private double sampleNormalDistribution()
        {
            //This is from a stack overflow question
            double u1 = r.NextDouble();
            double u2 = r.NextDouble();
            double randStdNormal = Math.Sqrt( -2.0 * Math.Log( u1 ) ) * Math.Sin( 2.0 * Math.PI * u2 ); //random normal(0,1)
            double randNormal = (MAX_ELO - MIN_ELO) / 2d;

            bool plusMinus = RandomBool( 50 );

            if ( plusMinus )
                randNormal += STD_DEV * randStdNormal;
            else
                randNormal -= STD_DEV * randStdNormal;

            if ( randNormal < 0 )
                Console.WriteLine( "Bad sample" );

            return randNormal;
        }

        /// <summary>
        /// Simulate the games and update Elo scores for the players in the games
        /// </summary>
        /// <param name="games">Games to play</param>
        private void playGames( List<Tuple<Team, Team>> games )
        {
            //Parallel.ForEach( games, x => { bool winner = decideWinner( x ); updateElos( x, winner ); } );

            //Play the games
            foreach ( Tuple<Team, Team> game in games )
            {
                //Decide a winner
                bool winner = decideWinner( game );
                //Update the players' Elos
                updateElos( game, winner );
            }
        }

        /// <summary>
        /// Find the player with the highest Elo
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private Player findTopPlayer( List<Player> players )
        {
            int max = int.MinValue;
            Player m = null;

            foreach ( Player p in players )
            {
                if ( p.GetElo() > max )
                {
                    max = p.GetElo();
                    m = p;
                }
            }

            return m;
        }

        /// <summary>
        /// Find the player with the lowest Elo
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private Player findBottomPlayer( List<Player> players )
        {
            int min = int.MaxValue;
            Player m = null;

            foreach ( Player p in players )
            {
                if ( p.GetElo() < min )
                {
                    min = p.GetElo();
                    m = p;
                }
            }

            return m;
        }

        /// <summary>
        /// Decide which team wins a match based upon team Elo scores
        /// </summary>
        /// <param name="game"></param>
        /// <returns>True if team 1 one, False if team 2 won</returns>
        private bool decideWinner( Tuple<Team, Team> game )
        {
            //double difference = game.Item1.GetAverageElo() - game.Item2.GetAverageElo();
            double difference = (SUM_ELOS) ? (double)game.Item1.GetRealTotalElo() - (double)game.Item2.GetRealTotalElo() : 
                                             (double)game.Item1.GetRealAverageElo() - (double)game.Item2.GetRealAverageElo();
            //double divisor = SUM_ELOS ? 400 * NUM_PLAYERS : 400;
            double divisor = 400;
            double e = 1d / (1d + Math.Pow( 10d, -difference / divisor ));

            double result = r.NextDouble();

            //if result < e, then team 1 won, otherwise team 2 won
            return result < e;
        }

        /// <summary>
        /// Update the Elo scores for players
        /// </summary>
        /// <param name="game"></param>
        /// <param name="winner"></param>
        private void updateElos( Tuple<Team, Team> game, bool winner )
        {
            //Fairly standard Elo implementation
            double difference = game.Item1.GetAverageElo() - game.Item2.GetAverageElo();
            double e = 1d / (1d + Math.Pow( 10d, -difference / 400d ));
            double winFlag1 = (winner) ? 1 : 0;
            double winFlag2 = (!winner) ? 1 : 0;

            _eloDiffs.Add( difference );

            foreach ( Player p in game.Item1.Players )
            {
                p.ChangeElo( (int)Math.Round( p.GetElo() + K * (winFlag1 - e) ) );

                if ( winner )
                    p.Wins++;
                else
                    p.Losses++;
            }
            //p.ChangeElo( p.GetElo() + tempK / Math.Pow( 1 + (double)p.GetGameCount(), ELO_UPDATE_VALUE ) );

            foreach ( Player p in game.Item2.Players )
            {
                p.ChangeElo( (int)Math.Round( p.GetElo() + K * (winFlag2 - e) ) );

                if ( !winner )
                    p.Wins++;
                else
                    p.Losses++;
            }
            //p.ChangeElo( p.GetElo() - tempK / Math.Pow( 1 + (double)p.GetGameCount(), ELO_UPDATE_VALUE ) );
        }

        /// <summary>
        /// Write Elo scores to the console
        /// </summary>
        /// <param name="players"></param>
        private void dumpElos( List<Player> players )
        {
            foreach ( Player p in players )
            {
                Console.WriteLine( "Player " + p.ID + " has played " + p.GetGameCount() + " games and has an Elo of " + p.GetElo() );
            }
        }

        /// <summary>
        /// Write Elo scores to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="players"></param>
        private void writeElos( String path, List<Player> players )
        {
            using ( TextWriter writer = new StreamWriter( path ) )
            {
                writer.WriteLine( "Id,Elo,Games,RealElo,Wins,Losses,EloDiff" );

                foreach ( Player p in players )
                {
                    writer.WriteLine( p.ID + "," + p.GetElo() + "," + p.GetGameCount() + "," + p.RealElo + "," + p.Wins + "," + p.Losses + "," + p.GetEloDifference() );
                }
            }
        }

        /// <summary>
        /// Write binned Elo scores to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bins"></param>
        private void writeBins( String path, Dictionary<int, List<Player>> bins )
        {
            using ( TextWriter writer = new StreamWriter( path ) )
            {
                writer.WriteLine( "bin,count" );

                foreach ( int bin in bins.Keys )
                {
                    writer.WriteLine( bin + "," + bins[bin].Count );
                }
            }
        }

        /// <summary>
        /// Write differences in Elo scores between teams to a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="differences"></param>
        private void writeEloDifferences( String path, List<Double> differences )
        {
            using ( TextWriter writer = new StreamWriter( path ) )
            {
                writer.WriteLine( "difference" );

                foreach ( double diff in differences )
                {
                    writer.WriteLine( diff );
                }
            }
        }

        /// <summary>
        /// Write some game stats to the console
        /// </summary>
        /// <param name="players"></param>
        private void dumpStats( List<Player> players )
        {
            double max = findTopPlayer( players ).GetElo();
            double min = findBottomPlayer( players ).GetElo();

            Console.WriteLine( "Elo ranges from " + min + " to " + max );

            double averageNumGames = players.Average( x => x.GetGameCount() );

            Console.WriteLine( "Average number of games played: " + averageNumGames );
        }

        /// <summary>
        /// Generate a bool value with the specified chance of true
        /// </summary>
        /// <param name="chanceOfTrue"></param>
        /// <returns></returns>
        public static bool RandomBool( int chanceOfTrue = 50 )
        {
            int num = r.Next( 100 );

            return num < chanceOfTrue;
        }

        /// <summary>
        /// Divide Elo scores into bins
        /// </summary>
        /// <param name="players"></param>
        /// <param name="binSize"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private Dictionary<int, List<Player>> binElos( List<Player> players, int binSize, int min, int max )
        {
            Dictionary<int, List<Player>> bins = new Dictionary<int, List<Player>>();

            foreach ( Player p in players )
            {
                int bin = findBin( (int)p.GetElo(), binSize, min, max );

                if ( !bins.ContainsKey( bin ) )
                    bins.Add( bin, new List<Player>() );

                bins[bin].Add( p );
            }

            return bins;
        }

        /// <summary>
        /// Determine the bin for the specified score
        /// </summary>
        /// <param name="number"></param>
        /// <param name="binSize"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int findBin( int number, int binSize, int min, int max )
        {
            return ((number - min) / binSize) * binSize;
        }

        /// <summary>
        /// Get a random subset of players
        /// </summary>
        /// <param name="players"></param>
        /// <param name="subsetSize"></param>
        /// <returns></returns>
        private List<Player> getSubset( List<Player> players, int subsetSize )
        {
            List<Player> t = players.RandomOrdering( r, subsetSize ).ToList();

            return t;

            //if ( t.Count > subsetSize )
            //    return t.GetRange( 0, subsetSize );
            //else
            //    return players;
        }

        /// <summary>
        /// Check if either team is an EmptyTeam
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool isEmptyGame( Tuple<Team, Team> game )
        {
            //Check if either team is an EmptyTeam
            return game.Item1 is EmptyTeam || game.Item2 is EmptyTeam;
        }
    }
}
