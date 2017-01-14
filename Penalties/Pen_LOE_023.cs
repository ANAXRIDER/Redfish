using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Pen_LOE_023 : PenTemplate //darkpeddler
	{
		public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
		{
            if (p.ownMaxMana == 1) return 10;
			return 0;
		}
	}
}
