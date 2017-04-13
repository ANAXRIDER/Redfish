using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_856 : SimTemplate //Hallucination
    {

        //Discover a card from your opponent's class.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.CardToHand(CardDB.cardName.unknown, true);
        }

    }

}