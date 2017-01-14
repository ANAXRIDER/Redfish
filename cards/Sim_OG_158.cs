using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_OG_158 : SimTemplate //* Zealous Initiate
	{
		//Deathrattle: Give a random friendly minion +1/+1.
		
        public override void onDeathrattle(Playfield p, Minion m)
        {
            //List<Minion> temp = new List<Minion>(p.ownMinions);
            //temp.Sort((a, b) => a.Angr.CompareTo(b.Angr));//take the weakest

            //if (temp.Count >= 1)
            //{
            //    for (int i = 1; i <= temp.Count; i++)
            //    {
            //        if (temp[i - 1].playedThisTurn || !temp[i - 1].Ready)
            //        {
            //            temp.RemoveAt(i - 1);
            //        }
            //    }
            //}
            




            Minion target = (m.own) ? p.searchRandomMinion(p.ownMinions, Playfield.searchmode.searchLowestAttack) : p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchLowestAttack);

            //if (target.playedThisTurn)
            //{
            //    target = temp[0];
            //}

            

            if (target != null) p.minionGetBuffed(target, 1, 1);
        }
    }
}