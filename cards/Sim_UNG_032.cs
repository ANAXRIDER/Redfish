using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_032 : SimTemplate //Crystalline Oracle
    {

        //Deathrattle: Copy a cardfrom your opponent's deck_and add it to your hand.

        public override void onDeathrattle(Playfield p, Minion m)
        {
            if (p.enemyDeckSize >= 1) p.CardToHand(CardDB.cardName.unknown, m.own);
        }

    }

}