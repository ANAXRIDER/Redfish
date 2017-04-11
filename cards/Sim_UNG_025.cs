using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_025 : SimTemplate //Volcano
    {

        //Deal $15 damage randomly split among all_minions.Overload: (2)

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<Minion> targets = new List<Minion>(p.enemyMinions);
            targets.AddRange(p.ownMinions);

            int times = (ownplay) ? p.getSpellDamageDamage(15) : p.getEnemySpellDamageDamage(15);

            if (ownplay)
            {
                targets.Sort((a, b) => -a.Hp.CompareTo(b.Hp));  // most hp -> least
            }
            else
            {
                targets.Sort((a, b) => a.Hp.CompareTo(b.Hp));  // least hp -> most
            }

            // Distribute the damage evenly among the targets
            int i = 0;
            while (i < times)
            {
                int loc = i % targets.Count;
                p.minionGetDamageOrHeal(targets[loc], 1);
                i++;
            }

            p.changeRecall(ownplay, 2);
        }

    }

}