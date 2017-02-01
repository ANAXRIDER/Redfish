using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_CFM_308 : SimTemplate //* Kun the Forgotten King
	{
		// Choose One - Gain 10 Armor; or Refresh your Mana Crystals.

        public override void getBattlecryEffect(Playfield p, Minion m, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && m.own))
            {
                p.minionGetArmor(m.own ? p.ownHero : p.enemyHero, 10);
            }

            if (choice == 2 || (hasfandral && m.own))
            {
                if (m.own) p.mana = p.ownMaxMana;
                else p.mana = p.enemyMaxMana;
            }
        }
    }
}