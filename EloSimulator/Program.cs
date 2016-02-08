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
    class Program
    {
        static void Main( string[] args )
        {
            Random r = new Random();
            ITeamChooser teamChooser = new SeededTeamChooser( new SimpleTeamDivider(), new SimplePlayerFinder() ) { MaxElo = 2800, MinElo = 0, EloRange = 400 };
            ISeedChooser seedChooser = new SimpleSeedChooser();
            Simulator s = new Simulator( teamChooser, new SimpleAllTeamsChooser( teamChooser, seedChooser ) );

            ConsoleKeyInfo key = new ConsoleKeyInfo();

            Console.WriteLine( "Press Q to Quit.\n" );

            while ( key.Key != ConsoleKey.Q )
            {
                //Play some games
                s.RunOnly( 12, 100, seedChooser );

                Console.WriteLine( "Press any key except Q to play more games.\n" );
                key = Console.ReadKey();
            }

            Console.WriteLine( "End" );
        }
    }
}
