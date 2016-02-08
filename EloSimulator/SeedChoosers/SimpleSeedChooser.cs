using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Choose the first player
    /// </summary>
    public class SimpleSeedChooser : ISeedChooser
    {
        /// <summary>
        /// 
        /// </summary>
        private ICollection<Player> _ignoredPlayers = new HashSet<Player>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public Player ChooseSeed( List<Player> players )
        {
            return chooseSeed( players, _ignoredPlayers );
        }

        /// <summary>
        /// Finds the first player that isn't ignored
        /// </summary>
        /// <param name="players"></param>
        /// <param name="ignoredPlayers"></param>
        /// <returns></returns>
        private Player chooseSeed( List<Player> players, ICollection<Player> ignoredPlayers )
        {
            if ( players.Count > 0 )
                return players.First( x => !ignoredPlayers.Contains( x ) );

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
