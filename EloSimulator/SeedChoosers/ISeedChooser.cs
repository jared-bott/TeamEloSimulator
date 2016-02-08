using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public interface ISeedChooser
    {
        void Initialize();
        Player ChooseSeed( List<Player> players );
        void IgnorePlayer( Player player );
    }
}
