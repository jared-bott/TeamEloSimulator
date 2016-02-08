using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    /// <summary>
    /// Choose the player with the highest Elo
    /// </summary>
    public class TopSeedChooser : ISeedChooser
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
            Player player = findTopPlayer( players, _ignoredPlayers );

            return player;
        }

        /// <summary>
        /// Find the player with the highest Elo
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private Player findTopPlayer( List<Player> players, ICollection<Player> ignoredPlayers )
        {
            int max = int.MinValue;
            Player m = null;

            foreach ( Player p in players )
            {
                if ( !ignoredPlayers.Contains( p ) )
                {
                    if ( p.GetElo() > max )
                    {
                        max = p.GetElo();
                        m = p;
                    }
                }
            }

            return m;
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
