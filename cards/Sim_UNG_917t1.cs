using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_917t1 : SimTemplate //Dinomancy
    {

        //Hero Power Give a Beast +2/+2.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
            List<Minion> tempBeast = new List<Minion>();
            foreach (Minion m in temp)
            {
                if ((TAG_RACE)m.handcard.card.race == TAG_RACE.BEAST)
                {
                    tempBeast.Add(m);
                }
            }
            if (tempBeast.Count >= 1)
            {
                p.minionGetBuffed(p.searchRandomMinion(tempBeast, (ownplay ? Playfield.searchmode.searchLowestHP : Playfield.searchmode.searchHighestHP)), 2, 2);
            }
        }

    }

}