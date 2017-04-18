using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_116t : SimTemplate //Barnabus the Stomper
    {

        //Battlecry: Reduce theCost of minions in your deck to (0).

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (own.own) p.ownMinionsCost0 = true;
        }

    }

}