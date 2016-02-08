using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public interface IPlayerFinder
    {
        List<Player> FindPlayers( List<Player> players, double maxElo, int eloRange );
    }
}
