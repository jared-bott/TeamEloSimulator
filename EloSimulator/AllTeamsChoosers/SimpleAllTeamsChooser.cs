using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Divide all players into Teams
    /// </summary>
    public class SimpleAllTeamsChooser : IAllTeamsChooser
    {
        /// <summary>
        /// 
        /// </summary>
        public ITeamChooser TeamChooser { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public ISeedChooser SeedChooser { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamChooser"></param>
        /// <param name="seedChooser"></param>
        public SimpleAllTeamsChooser( ITeamChooser teamChooser, ISeedChooser seedChooser )
        {
            if ( teamChooser != null )
                TeamChooser = teamChooser;
            else
                throw new NullReferenceException();

            if ( seedChooser != null )
                SeedChooser = seedChooser;
            else
                SeedChooser = new SimpleSeedChooser();
        }

        /// <summary>
        /// Divide all players into Teams
        /// </summary>
        /// <param name="players"></param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        public List<Tuple<Team, Team>> ChooseAllTeams( List<Player> players, int playersPerTeam )
        {
            return chooseAllTeams( players, TeamChooser, SeedChooser, playersPerTeam );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="teamChooser"></param>
        /// <param name="seedChooser"></param>
        /// <param name="playersPerTeam"></param>
        /// <returns></returns>
        private List<Tuple<Team, Team>> chooseAllTeams( List<Player> players, ITeamChooser teamChooser, ISeedChooser seedChooser, int playersPerTeam )
        {
            List<Tuple<Team, Team>> games = new List<Tuple<Team, Team>>();
            List<Player> t = new List<Player>( players );
            List<Player> played = new List<Player>();
            int count = 0;

            seedChooser.Initialize();

            while ( t.Count >= 2 * playersPerTeam )
            {
                Player seed = seedChooser.ChooseSeed( t );

                if ( seed != null )
                {
                    Tuple<Team, Team> game = teamChooser.ChooseTeams( t, seed, playersPerTeam );
                    count++;

                    if ( game == null || isEmptyGame( game ) )
                    {
                        seedChooser.IgnorePlayer( seed );

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
            }

            return games;
        }

        /// <summary>
        /// Check if either Team in a game is an EmptyTeam
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private bool isEmptyGame( Tuple<Team, Team> game )
        {
            return game.Item1 is EmptyTeam || game.Item2 is EmptyTeam;
        }
    }
}
