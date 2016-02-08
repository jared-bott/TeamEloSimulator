using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public interface IAllTeamsChooser
    {
        List<Tuple<Team, Team>> ChooseAllTeams( List<Player> players, int playersPerTeam );
    }
}
