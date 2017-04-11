using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_022 : SimTemplate //Mirage Caller
    {

        //Battlecry: Choose a friendly minion. Summon a 1/1 copy of it.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
            if (target != null)
            {
                p.callKid(target.handcard.card, own.zonepos, own.own, true);
                Minion kidminion = temp[own.zonepos-1];
                int angr = 1 - kidminion.Angr;
                int hp = 1 - kidminion.maxHp;
                p.minionGetBuffed(kidminion, angr, hp);
            }
        }
    }

}