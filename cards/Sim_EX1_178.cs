using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_178 : SimTemplate //ancientofwar
    {
        //Choose One - +5 Attack; or +5 Health and Taunt.
        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && own.own))
            {
                p.minionGetBuffed(own, 5, 0);
            }
            if (choice == 2 || (hasfandral && own.own))
            {
                p.minionGetBuffed(own, 0, 5);
                own.taunt = true;
            }
        }
    }
}