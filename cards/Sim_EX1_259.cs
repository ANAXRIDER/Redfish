using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_259 : SimTemplate//Lightning Storm
    {

        //Deal $2-$3 damage to all enemy minions. Overload: (2)
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.changeRecall(ownplay, 2);
            int dmg = (ownplay) ? p.getSpellDamageDamage(2) : p.getEnemySpellDamageDamage(2);
            p.allMinionOfASideGetDamage(!ownplay, dmg);
        }
    }
}
