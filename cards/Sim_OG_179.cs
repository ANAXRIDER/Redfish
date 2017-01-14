using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_OG_179 : SimTemplate //* Fiery Bat
	{
		//Deathrattle: Deal 1 damage to a random enemy.
		
        public override void onDeathrattle(Playfield p, Minion m)
        {
			Minion target = (m.own)? p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchLowestHP) : p.searchRandomMinion(p.ownMinions, Playfield.searchmode.searchLowestHP);
			if (target == null) target = (m.own) ? p.enemyHero : p.ownHero;
            //Helpfunctions.Instance.ErrorLog("target = " + target.entityID);
			p.minionGetDamageOrHeal(target, 1);
        }
    }
}