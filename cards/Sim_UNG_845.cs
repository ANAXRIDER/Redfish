using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_845 : SimTemplate //Igneous Elemental
    {

        //Deathrattle: Add two 1/2 Elementals to your hand.

        public override void onDeathrattle(Playfield p, Minion m)
        {
            p.CardToHand(CardDB.cardName.flameelemental, m.own);
            p.CardToHand(CardDB.cardName.flameelemental, m.own);
        }

    }

}