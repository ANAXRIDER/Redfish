using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_955 : SimTemplate //Meteor
    {

        //Deal $15 damage to a minion and $3 damage to adjacent ones.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            int dmg1 = (ownplay) ? p.getSpellDamageDamage(15) : p.getEnemySpellDamageDamage(15);
            int dmg2 = (ownplay) ? p.getSpellDamageDamage(3) : p.getEnemySpellDamageDamage(3);
            List<Minion> temp = (target.own) ? p.ownMinions : p.enemyMinions;
            p.minionGetDamageOrHeal(target, dmg1);
            foreach (Minion m in temp)
            {
                if (m.zonepos + 1 == target.zonepos || m.zonepos - 1 == target.zonepos) p.minionGetDamageOrHeal(m, dmg2);
            }
        }

    }

}