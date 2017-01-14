using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Pen_EX1_319 : PenTemplate //flameimp
	{
		public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
		{
            int ret = -3;
            if (p.ownMaxMana == 1) ret -= 2;
            else if (p.enemyMinions.Count == 0 && p.manaTurnEnd >= hc.getManaCost(p) && p.anzMinionsDiedThisTurn == 0) ret -= 2;

            //Helpfunctions.Instance.ErrorLog("p.enemyMinionsDiedTurn " + p.anzMinionsDiedThisTurn);

            if (p.ownMaxMana == 1 && (p.enemyHeroName == HeroEnum.hunter || p.enemyHeroName == HeroEnum.warrior))
            {
                foreach (Handmanager.Handcard hcc in p.owncards)
                {
                    if (hcc.card.name == CardDB.cardName.possessedvillager ||
                        hcc.card.name == CardDB.cardName.voidwalker ||
                        hcc.card.name == CardDB.cardName.argentsquire) ret = 10;
                }
            }

            if (p.ownMaxMana == 1)
            {
                bool enemyhas2attack = false;
                foreach (Minion mn in p.enemyMinions)
                {
                    if (mn.Angr >= 2) enemyhas2attack = true;
                }
                bool hastaunt = false;
                foreach (Minion mn in p.ownMinions)
                {
                    if (mn.taunt) hastaunt = true;
                }

                if (enemyhas2attack && !hastaunt && p.ownMinions.Count >= 1) ret += 15;
            }

            return ret;
		}
	}
}
