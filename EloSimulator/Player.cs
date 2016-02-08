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

namespace EloSimulator
{
    /// <summary>
    /// 
    /// </summary>
    public class Player
    {
        private static readonly int START_ELO = 1300;
        private static readonly int MIN_ELO = 0;
        private static readonly int MAX_ELO = 2800;

        private int _currentElo = int.MinValue;

        /// <summary>
        /// Player ID
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Player Elo
        /// </summary>
        public Stack<int> Elo
        {
            get;
            set;
        }

        public Player( int id )
        {
            ID = id;
            Elo = new Stack<int>();
            ChangeElo( START_ELO );
        }

        /// <summary>
        /// Get the most recent Elo
        /// </summary>
        /// <returns></returns>
        public int GetElo()
        {
            //if ( Elo.Count > 0 )
            //    return Elo.Peek();
            //else
            //    return int.MinValue;

            return _currentElo;
        }

        /// <summary>
        /// Change the player's Elo
        /// </summary>
        /// <param name="elo"></param>
        public void ChangeElo( int elo )
        {
            int tempElo = elo;

            if ( elo < MIN_ELO )
                tempElo = MIN_ELO;
            else if ( elo > MAX_ELO )
                tempElo = MAX_ELO;

            Elo.Push( tempElo );
            _currentElo = tempElo;
        }

        /// <summary>
        /// Get the number of games played
        /// </summary>
        /// <returns></returns>
        public int GetGameCount()
        {
            return Elo.Count - 1;
        }

        /// <summary>
        /// Get the difference between the player's real Elo and their calculated Elo
        /// </summary>
        /// <returns></returns>
        public int GetEloDifference()
        {
            int diff = GetElo() - RealElo;

            return diff;
        }

        /// <summary>
        /// Get the player's real Elo
        /// </summary>
        public int RealElo
        {
            get;
            set;
        }

        /// <summary>
        /// Number of games won
        /// </summary>
        public int Wins
        {
            get;
            set;
        }

        /// <summary>
        /// Number of games lost
        /// </summary>
        public int Losses
        {
            get;
            set;
        }
    }
}
