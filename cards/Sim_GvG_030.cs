using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_GVG_030 : SimTemplate //Anodized Robo Cub
    {

        //    Taunt. Choose One - +1 Attack; or +1 Health.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && own.own))
            {
                p.minionGetBuffed(own, 1, 0);
            }
            if (choice == 2 || (hasfandral && own.own))
            {
                p.minionGetBuffed(own, 0, 1);
            }
        }
    }

}