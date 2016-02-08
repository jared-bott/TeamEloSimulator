using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public interface ITeamDivider
    {
        Tuple<Team, Team> DividePlayers( List<Player> players, int playersPerTeam, Player seed );
    }
}
