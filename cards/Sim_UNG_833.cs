using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_833 : SimTemplate //Lakkari Felhound
    {

        //Taunt  Battlecry: Discard two random cards.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            p.discardACard(own.own);
            p.discardACard(own.own);
        }

    }

}