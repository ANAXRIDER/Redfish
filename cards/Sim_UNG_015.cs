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
                p.minionSetAngrToX(minion, 3);
                p.minionSetLifetoX(minion, 3);
            }

            foreach (Minion minion in p.enemyMinions)
            {
                p.minionSetAngrToX(minion, 3);
                p.minionSetLifetoX(minion, 3);
            }
        }
    }

}