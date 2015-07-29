﻿/*
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
        private static readonly int ELO_RANGE = 400;
        private static readonly int K = 50;
        //private static readonly double ELO_UPDATE_VALUE = 0.2d;
        private static readonly int NUM_GAMES = 1000;
        private static readonly int NUM_PLAYERS = 2000;
        private static readonly int NUM_CLOSE_GAME = 4;
        private static readonly bool USE_RANDOM_TEAMS = false;
        //private static readonly int NUM_SIMULTANEOUS_GAMES = 5;
        private static readonly int NUM_PLAYERS_ONLINE_AT_ONCE = 1000;
        private static readonly int MIN_ELO = 0;
        private static readonly int MAX_ELO = 2800;
        private static readonly double STD_DEV = 300;
        private static readonly bool BUCKET_TEAMS = false;
        private static readonly bool SUM_ELOS = false;

        public Simulator()
        {
            initializePlayers( NUM_PLAYERS );
            //List<int> test = Enumerable.Range( 0, 100 ).ToList();
            //List<int> p = RandomOrdering<int>( test, 3 );
        }

        /// <summary>
        /// Run the number of games specified by NUM_GAMES
        /// </summary>
        public void Run()
        {
            Run( NUM_GAMES );
        }

        /// <summary>
        /// Run some number of games
        /// </summary>
        /// <param name="numGames">The number of games to run</param>
        public void Run( int numGames )
        {
            for ( int count = 0; count < numGames; count++ )
            {
                List<Tuple<Team, Team>> games = chooseTeamsAll( _players, PLAYERS_PER_TEAM );
                playGames( games );
            }

            //Save information about the games
            outputGameInfo( numGames );
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
            //List<Player> seedPlayers = new List<Player>();
            List<Player> t = RandomOrdering<Player>( _players );

            //for ( int count = 0; count < numGames; count++ )
            //seedPlayers.Add( t[count] );

            List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();

            int count = 0;
            //foreach ( Player p in seedPlayers )
            while ( count < numGames )
            {
                Tuple<Team, Team> game = chooseTeams( _players, PLAYERS_PER_TEAM, t[count], ELO_RANGE );

                if ( game != null )
                {
                    games.Add( game );

                    foreach ( Player p in game.Item1.Players )
                        t.Remove( p );

                    foreach ( Player p in game.Item2.Players )
                        t.Remove( p );
                }

                count++;
                //games.Add( chooseTeams( _players, PLAYERS_PER_TEAM, p ) );
            }

            playGames( games );

            outputGameInfo( numGames );
        }

        /// <summary>
        /// Run multiple rounds of simultaneous games so that each player is only in one game at a time.
        /// </summary>
        /// <param name="numGames">The number of games to run simultaneously</param>
        /// <param name="numTimes">The number of rounds of simultaneous games</param>
        public void RunOnly( int numGames, int numTimes )
        {
            for ( int i = 0; i < numTimes; i++ )
            {
                //Play a round of matches

                //List<Player> seedPlayers = new List<Player>();
                List<Player> t = RandomOrdering<Player>( _players );

                //for ( int count = 0; count < numGames; count++ )
                //seedPlayers.Add( t[count] );

                List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();

                int count = 0;
                int index = 0;

                List<Player> players = getSubset( t, NUM_PLAYERS_ONLINE_AT_ONCE );

                //foreach ( Player p in seedPlayers )
                while ( count < numGames )
                {
                    Tuple<Team, Team> game = null;

                    if ( BUCKET_TEAMS )
                        game = chooseTeamsByBuckets( players, PLAYERS_PER_TEAM, players[index] );
                    else
                        game = chooseTeams( players, PLAYERS_PER_TEAM, players[index], ELO_RANGE );
                    
                    //game = chooseTeamsLoosely( _players, PLAYERS_PER_TEAM, t[count] );

                    if ( game != null )
                    {
                        games.Add( game );

                        foreach ( Player p in game.Item1.Players )
                            players.Remove( p );

                        foreach ( Player p in game.Item2.Players )
                            players.Remove( p );
                    }
                    else
                    {
                        if ( index + 1 < players.Count - 1 )
                            index++;
                    }

                    count++;
                    //games.Add( chooseTeams( _players, PLAYERS_PER_TEAM, p ) );
                }

                playGames( games );
            }

            outputGameInfo( numGames * numTimes );
        }

        /// <summary>
        /// Create some players
        /// </summary>
        /// <param name="numPlayers">The number of players to create</param>
        private void initializePlayers( int numPlayers )
        {
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
            double u1 = r.NextDouble(); //these are uniform(0,1) random doubles
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
        /// Assign as many players into teams as possible
        /// </summary>
        /// <param name="players">Players from which to create matches</param>
        /// <param name="playersPerTeam">Number of players on each team</param>
        /// <returns></returns>
        private List<Tuple<Team, Team>> chooseTeamsAll( List<Player> players, int playersPerTeam )
        {
            List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();
            List<Player> t = new List<Player>( players );
            int count = 0;

            List<Player> played = new List<Player>();

            while ( t.Count >= 2 * playersPerTeam /*&& count < players.Count / playersPerTeam*/ )
            {
                Tuple<Team, Team> game = (USE_RANDOM_TEAMS) ? chooseTeamsRandomly( t, playersPerTeam ) : chooseTeamsFromTopPlayer( t, playersPerTeam );
                count++;

                if ( game == null )
                {
                    if ( !USE_RANDOM_TEAMS )
                    {
                        Player p = findTopPlayer( t );
                        t.Remove( p );
                    }

                    continue;
                }

                games.Add( game );

                foreach ( Player p in game.Item1.Players )
                {
                    if ( played.Contains( p ) )
                        Console.WriteLine( "Bad!" );

                    t.Remove( p );
                    played.Add( p );
                }

                foreach ( Player p in game.Item2.Players )
                {
                    if ( played.Contains( p ) )
                        Console.WriteLine( "Bad!" );

                    t.Remove( p );
                    played.Add( p );
                }
            }

            return games;
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
        /// Choose the teams based upon the available player with the highest Elo
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeamsFromTopPlayer( List<Player> players, int playersPerTeam )
        {
            //Tuple<Team, Team> game=new Tuple<Team,Team>(
            Player topPlayer = findTopPlayer( players );
            return chooseTeams( players, playersPerTeam, topPlayer, ELO_RANGE );
        }

        /// <summary>
        /// Choose the teams based upon a seed player
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <param name="eloRange"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeams( List<Player> players, int playersPerTeam, Player seed, int eloRange )
        {
            int max = 0;
            int min = 0;

            if ( seed.GetElo() + eloRange / 2 > MAX_ELO )
            {
                max = MAX_ELO;
                min = seed.GetElo() - (eloRange / 2) - (MAX_ELO - seed.GetElo());
            }
            else
            {
                if ( seed.GetElo() - eloRange / 2 < MIN_ELO )
                {
                    min = MIN_ELO;
                    max = seed.GetElo() + (eloRange / 2) + (seed.GetElo() - MIN_ELO);
                }
                else
                {
                    max = seed.GetElo() + eloRange / 2;
                    min = seed.GetElo() - eloRange / 2;
                }
            }

            List<Player> possible = findPossiblePlayers( players, max, max - min );

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
                return dividePlayers( possible, playersPerTeam, seed );
            }
            else
            {
                //Not enough players, expand the Elo range
                if ( eloRange < MAX_ELO - MIN_ELO )
                    return chooseTeams( players, playersPerTeam, seed, eloRange * 2 );
            }

            return null;
        }

        /// <summary>
        /// Choose teams based upon Elo scores, but allowing for a greater range of scores per team
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeamsLoosely( List<Player> players, int playersPerTeam, Player seed )
        {
            List<Player> possible = findPossiblePlayers( players, seed.GetElo(), ELO_RANGE );

            if ( !possible.Contains( seed ) )
            {
                possible.Add( seed );
            }

            if ( possible.Count >= 2 * playersPerTeam )
            {
                possible.Remove( seed );

                return dividePlayersLoosely( possible, playersPerTeam, seed, NUM_CLOSE_GAME );
            }

            return null;
        }

        /// <summary>
        /// Choose teams based upon placement into Elo buckets
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeamsByBuckets( List<Player> players, int playersPerTeam, Player seed )
        {
            List<Player> possible = findPossiblePlayers( getSubset( players, NUM_PLAYERS_ONLINE_AT_ONCE ), seed.GetElo(), ELO_RANGE );

            if ( !possible.Contains( seed ) )
                possible.Add( seed );

            if ( possible.Count >= 2 * playersPerTeam )
            {
                possible.Remove( seed );

                return bucketPlayers( possible, playersPerTeam, seed );
            }

            return null;
        }

        /// <summary>
        /// Choose teams through random assignment
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        private Tuple<Team, Team> chooseTeamsRandomly( List<Player> players, int playersPerTeam )
        {
            if ( players.Count >= 2 * playersPerTeam )
            {
                List<Player> t = RandomOrdering<Player>( players, 2 * playersPerTeam );
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

            return null;
        }

        /// <summary>
        /// Find the player with the highest Elo
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private Player findTopPlayer( List<Player> players )
        {
            //players.Sort( ( p1, p2 ) => p1.getElo().CompareTo( p2.getElo() ) );
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
        /// Find players that fit within the min and max Elo scores
        /// </summary>
        /// <param name="players"></param>
        /// <param name="maxElo"></param>
        /// <param name="eloRange"></param>
        /// <returns></returns>
        private List<Player> findPossiblePlayers( List<Player> players, double maxElo, int eloRange )
        {
            List<Player> possible = players.FindAll( x => x.GetElo() <= maxElo && x.GetElo() >= maxElo - eloRange );

            return possible;//.Distinct().ToList();
        }

        /// <summary>
        /// Divide players into two teams
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private Tuple<Team, Team> dividePlayers( List<Player> players, int playersPerTeam, Player seed )
        {
            //if ( players.Count > 2 * PLAYERS_PER_TEAM )
            //players.Sort( ( p1, p2 ) => p2.GetElo().CompareTo( p1.GetElo() ) );
            //players.Sort( ( p1, p2 ) => Math.Abs( seed.GetElo() - p2.GetElo() ) );
            int seedElo = seed.GetElo();
            players.Sort( ( p1, p2 ) => Math.Abs( seed.GetElo() - p1.GetElo() ).CompareTo( Math.Abs( seed.GetElo() - p2.GetElo() ) ) );
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

        /// <summary>
        /// Divide players into two teams with some players on each team having guaranteed close Elo scores, the rest assigned based upon total team Elo
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <param name="seed"></param>
        /// <param name="numClose"></param>
        /// <returns></returns>
        private Tuple<Team, Team> dividePlayersLoosely( List<Player> players, int playersPerTeam, Player seed, int numClose )
        {
            players.Sort( ( p1, p2 ) => p2.GetElo().CompareTo( p1.GetElo() ) );

            Team a = new Team();
            Team b = new Team();

            a.Players.Add( seed );

            for ( int count = 1; count < numClose; count++ )
            {
                if ( count % 2 == 0 )
                    a.Players.Add( players[count - 1] );
                else
                    b.Players.Add( players[count - 1] );
            }

            int numLeft = (2 * playersPerTeam) - (numClose + 1);

            players = RandomOrdering<Player>( players.GetRange( numClose, players.Count - numClose ) );

            for ( int count = 0; count < numLeft; count++ )
            {
                if ( a.Players.Count < playersPerTeam && b.Players.Count < playersPerTeam )
                {
                    double diffWithA = ((double)(a.GetTeamElo() + players[count].GetElo())) / (double)(a.Players.Count + 1) - b.GetAverageElo();
                    double diffWithB = ((double)(b.GetTeamElo() + players[count].GetElo())) / (double)(b.Players.Count + 1) - a.GetAverageElo();

                    if ( Math.Abs( diffWithA ) < Math.Abs( diffWithB ) )
                    {
                        a.Players.Add( players[count] );
                    }
                    else
                    {
                        b.Players.Add( players[count] );
                    }
                }
                else if ( a.Players.Count < playersPerTeam )
                    a.Players.Add( players[count] );
                else if ( b.Players.Count < playersPerTeam )
                    b.Players.Add( players[count] );
            }

            return new Tuple<Team, Team>( a, b );
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
            Team a = new Team();
            Team b = new Team();

            a.Players.Add( seed );

            //List<Player> temp = RandomOrdering<Player>( players );

            for ( int count = 1; count < 2 * playersPerTeam; count++ )
            {
                if ( count % 2 == 0 )
                    a.Players.Add( players[count - 1] );
                else
                    b.Players.Add( players[count - 1] );
            }

            return new Tuple<Team, Team>( a, b );
        }

        /// <summary>
        /// Decide which team wins a match based upon team Elo scores
        /// </summary>
        /// <param name="game"></param>
        /// <returns>True if team 1 one, False if team 2 won</returns>
        private bool decideWinner( Tuple<Team, Team> game )
        {
            //double difference = game.Item1.GetAverageElo() - game.Item2.GetAverageElo();
            double difference = (SUM_ELOS) ? (double)game.Item1.GetRealTotalElo() - (double)game.Item2.GetRealTotalElo() : (double)game.Item1.GetRealAverageElo() - (double)game.Item2.GetRealAverageElo();
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
        /// Perform Fisher-Yates shuffle on a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> RandomOrdering<T>( List<T> list )
        {
            List<T> array = new List<T>();
            array.AddRange( list );

            for ( int i = array.Count; i > 1; i-- )
            {
                int j = r.Next( i );
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }

            return array;
        }

        /// <summary>
        /// Perform Fisher-Yates shuffle on a subset of a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="numSubset"></param>
        /// <returns></returns>
        public static List<T> RandomOrdering<T>( List<T> list, int numSubset )
        {
            List<T> array = new List<T>();
            array.AddRange( list );

            int limit = (numSubset < array.Count) ? array.Count - numSubset : 1;

            for ( int i = array.Count; i > limit; i-- )
            {
                int j = r.Next( i );
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }

            if ( numSubset < array.Count )
                return array.GetRange( limit, numSubset );

            return array;
        }

        /// <summary>
        /// Generate a bool value with the specified chance of true
        /// </summary>
        /// <param name="chanceOfTrue"></param>
        /// <returns></returns>
        public static bool RandomBool( int chanceOfTrue )
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
            if ( min == 0 )
            {
                return (number / binSize) * binSize;
            }
            else
            {
                return ((number - min) / binSize) * binSize;
            }
        }

        /// <summary>
        /// Get a random subset of players
        /// </summary>
        /// <param name="players"></param>
        /// <param name="subsetSize"></param>
        /// <returns></returns>
        private List<Player> getSubset( List<Player> players, int subsetSize )
        {
            List<Player> t = RandomOrdering<Player>( players, subsetSize );

            return t;

            //if ( t.Count > subsetSize )
            //    return t.GetRange( 0, subsetSize );
            //else
            //    return players;
        }
    }
}
