using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_155 : SimTemplate //* markofnature
    {
        //Choose One - Give a minion +4 Attack; or +4 Health and Taunt.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && ownplay))
            {
                p.minionGetBuffed(target, 4, 0);
            }
            if (choice == 2 || (hasfandral && ownplay))
            {
                p.minionGetBuffed(target, 0, 4);
                if (!target.taunt)
                {
                    target.taunt = true;
                    if (target.own) p.anzOwnTaunt++;
                    else p.anzEnemyTaunt++;
                }
            }
        }
    }
}