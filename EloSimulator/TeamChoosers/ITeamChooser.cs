using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public interface ITeamChooser
    {
        Tuple<Team, Team> ChooseTeams( List<Player> players, Player seed, int playersPerTeam );
    }
}
