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
    public class Team
    {
        /// <summary>
        /// 
        /// </summary>
        public Team()
        {
            Players = new List<Player>();
        }

        /// <summary>
        /// Players on the team
        /// </summary>
        public List<Player> Players
        {
            get;
            set;
        }

        /// <summary>
        /// Get the total Elo for the team
        /// </summary>
        /// <returns></returns>
        public double GetTeamElo()
        {
            double elo = Players.Sum( x => x.GetElo() );

            return elo;
        }

        /// <summary>
        /// Get the average Elo for the team
        /// </summary>
        /// <returns></returns>
        public double GetAverageElo()
        {
            return GetTeamElo() / (double)Players.Count;
        }

        /// <summary>
        /// Get the average real Elo for the team
        /// </summary>
        /// <returns></returns>
        public double GetRealAverageElo()
        {
            return GetRealTotalElo() / (double)Players.Count;
        }

        /// <summary>
        /// Get the total real Elo for the team
        /// </summary>
        /// <returns></returns>
        public double GetRealTotalElo()
        {
            double elo = Players.Sum( x => x.RealElo );

            return elo;
        }
    }

    /// <summary>
    /// Placeholder class for an EmptyTeam that cannot have players
    /// </summary>
    public class EmptyTeam : Team
    {
        
    }
}
