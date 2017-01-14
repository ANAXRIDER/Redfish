using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Pen_EX1_059 : PenTemplate //crazedalchemist
	{
		public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
		{
            Playfield tmpPf = new Playfield();
            int ret = 5;



            if (target != null)
            {

                if (target.name == CardDB.cardName.darkshirecouncilman && target.own && target.Angr == 1) ret += 5;

                if (target.own && target.Angr <= 4) ret += 8 - 2 * target.Angr;

                if (p.playactions.Find(a => a.actionType == actionEnum.attackWithMinion && a.own.entityID == target.entityID && a.target.entityID == tmpPf.enemyHero.entityID) != null)
                {
                    ret += 10;
                }

                if (target.own)
                {
                    if ((!target.allreadyAttacked && target.Angr > target.Hp) || target.frozen || target.exhausted || !target.allreadyAttacked) ret += 10;
                }
            }

            return ret;
		}
	}
}
