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
    public class Player
    {
        private static readonly int START_ELO = 1300;
        private static readonly int MIN_ELO = 0;
        private static readonly int MAX_ELO = 2800;

        public int ID
        {
            get;
            set;
        }

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

        public int GetElo()
        {
            if ( Elo.Count > 0 )
                return Elo.Peek();
            else
                return int.MinValue;
        }

        public void ChangeElo( int elo )
        {
            if ( elo < MIN_ELO )
                Elo.Push( MIN_ELO );
            else if ( elo > MAX_ELO )
                Elo.Push( MAX_ELO );
            else
                Elo.Push( elo );
        }

        public int GetGameCount()
        {
            return Elo.Count - 1;
        }

        public int GetEloDifference()
        {
            int diff = GetElo() - RealElo;

            return diff;
        }

        public int RealElo
        {
            get;
            set;
        }

        public int Wins
        {
            get;
            set;
        }

        public int Losses
        {
            get;
            set;
        }
    }
}
