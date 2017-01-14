using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_LOE_046 : SimTemplate //huge toad
    {
        //Deathrattle: Deal 1 damage to a random enemy.

        public override void onDeathrattle(Playfield p, Minion m)
        {
            Minion target = (m.own) ? p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchLowestHP) : p.searchRandomMinion(p.ownMinions, Playfield.searchmode.searchLowestHP);
            if (target == null) target = (m.own) ? p.enemyHero : p.ownHero;
            p.minionGetDamageOrHeal(target, 1);
        }
    }
}