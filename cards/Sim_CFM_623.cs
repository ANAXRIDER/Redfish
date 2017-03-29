using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_CFM_623 : SimTemplate //* Greater Arcane Missiles
	{
		// Shoot three missiles at random enemies that deal 3 damage each.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            //int times = (ownplay) ? p.getSpellDamageDamage(3) : p.getEnemySpellDamageDamage(3);

            //while (times > 0)
            //{
            //    if (ownplay) target = p.getEnemyCharTargetForRandomSingleDamage(3);
            //    else
            //    {
            //        target = p.searchRandomMinion(p.ownMinions, Playfield.searchmode.searchHighestAttack); //damage the Highest (pessimistic)
            //        if (target == null) target = p.ownHero;
            //    }
            //    p.minionGetDamageOrHeal(target, 3);
            //    times--;
            //}

            int dmg = (ownplay) ? p.getSpellDamageDamage(3) : p.getEnemySpellDamageDamage(3);

            List<Minion> targets = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);

            if (ownplay)
            {
                targets.Add(p.enemyHero);
                targets.Sort((a, b) => -a.Hp.CompareTo(b.Hp));  // most hp -> least
            }
            else
            {
                targets.Add(p.ownHero);
                targets.Sort((a, b) => a.Hp.CompareTo(b.Hp));  // least hp -> most
            }

            // Distribute the damage evenly among the targets
            int i = 0;
            while (i < 3)
            {
                int loc = i % targets.Count;
                p.minionGetDamageOrHeal(targets[loc], 1);
                i++;
            }
        }
    }
}