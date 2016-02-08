using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Randomly choose a seed player
    /// </summary>
    public class RandomSeedChooser : ISeedChooser
    {
        /// <summary>
        /// 
        /// </summary>
        public Random Random { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private ICollection<Player> _ignoredPlayers = new HashSet<Player>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        public RandomSeedChooser( Random r )
        {
            if ( r != null )
                Random = r;
            else
                Random = new Random();
        }

        /// <summary>
        /// Choose a seed player from the list of players at random
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public Player ChooseSeed( List<Player> players )
        {
            return chooseSeed( players, Random );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private Player chooseSeed( List<Player> players, Random r )
        {
            if ( players.Count > 0 )
            {
                int index = r.Next( players.Count );

                if ( _ignoredPlayers.Contains( players[index] ) )
                {
                    //This could be improved with some heuristics on size of players versus ignored players
                    IEnumerable<Player> temp = players.RandomOrdering( r );

                    return temp.FirstOrDefault( x => !_ignoredPlayers.Contains( x ) );
                }
                else
                    return players[index];
            }

            return null;
        }

        /// <summary>
        /// Initialize seed chooser
        /// </summary>
        public void Initialize()
        {
            _ignoredPlayers.Clear();
        }

        /// <summary>
        /// Ignore a player for all subsequent calls to chooseSeed until Initialize is called
        /// </summary>
        /// <param name="player"></param>
        public void IgnorePlayer( Player player )
        {
            if ( player != null && !_ignoredPlayers.Contains( player ) )
                _ignoredPlayers.Add( player );
        }
    }
}
