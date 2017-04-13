using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_835 : SimTemplate //Chittering Tunneler
    {

        //Battlecry: Discover a spell. Deal damage to your hero equal to its Cost.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.CardToHand(CardDB.cardName.unknown, ownplay);

            p.minionGetDamageOrHeal(ownplay ? p.ownHero : p.enemyHero, 2); //assume 2-damage

        }
    }

}