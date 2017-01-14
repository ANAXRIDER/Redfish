using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_KAR_062 : SimTemplate //Netherspite Historian
    {
        // Battlecry: If you're holding a Dragon, Discover a Dragon.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasdragon = false;
            foreach (Handmanager.Handcard hc in p.owncards)
            {
                if (hc.card.race == TAG_RACE.DRAGON) hasdragon = true;
            }
            if (hasdragon) p.drawACard(CardDB.cardName.unknown, own.own);
        }
    }
}