using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_015 : SimTemplate //Sunkeeper Tarim
    {

        //Taunt Battlecry: Set all other minions' Attack and Health to 3.

        public override void getBattlecryEffect(Playfield p, Minion m, Minion target, int choice)
        {
            foreach (Minion minion in p.ownMinions)
            {
                int angr = 3 - minion.Angr;
                int hp = 3 - minion.maxHp;

                p.minionGetBuffed(minion, angr, hp);
            }

            foreach (Minion minion in p.enemyMinions)
            {
                int angr = 3 - minion.Angr;
                int hp = 3 - minion.maxHp;

                p.minionGetBuffed(minion, angr, hp);
            }
        }
    }

}