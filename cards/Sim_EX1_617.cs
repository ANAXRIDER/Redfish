using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_EX1_617 : SimTemplate //deadlyshot
	{
		public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
		{
            List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
            List<Minion> temp1 = (ownplay) ? new List<Minion>(p.ownMinions) : new List<Minion>(p.enemyMinions);
            temp2.Sort((a, b) => (a.Angr * 2 + a.Hp).CompareTo(b.Angr * 2 + b.Hp));
            //foreach (Minion enemy in temp2)
            //{
            //    p.minionGetDestroyed(enemy);
            //    break;
            //}

            if (temp2.Count <= 2)
            {
                p.minionGetDestroyed(temp2[0]);
            }
            else
            {
                bool trigger = false;
                foreach (Minion m in temp1)
                {
                    if (!m.playedThisTurn && m.Angr >= temp2[0].Hp) { p.minionGetDestroyed(temp2[0]); trigger = true;}
                }
                if (!trigger) p.minionGetDestroyed(temp2[temp2.Count / 2]);

            }
            
        }
	}
}