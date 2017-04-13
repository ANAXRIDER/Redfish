using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_937 : SimTemplate //Primalfin Lookout
    {

        //Battlecry: If you control another Murloc, Discover a_Murloc.

        public override void getBattlecryEffect(Playfield p, Minion m, Minion target, int choice)
        {
            if (p.ownMinions.Find(a => a.handcard.card.race == TAG_RACE.MURLOC) != null)
            {
                p.CardToHand(CardDB.cardName.unknown, true);
            }
        }

    }

}