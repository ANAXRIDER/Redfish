using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Pen_EX1_310 : PenTemplate //doomguard
	{
		public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
		{
            foreach (Handmanager.Handcard hcc in p.owncards)
            {
                if (hcc.card.type == CardDB.cardtype.MOB && hcc.canplayCard(p)) return 5;
            }
            return 0;
		}
	}
}
