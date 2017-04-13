using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_831 : SimTemplate //Corrupting Mist
    {

        //Corrupt every minion. Destroy them at the start of your next turn.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {

            foreach (Minion mnn in p.ownMinions)
            {
                mnn.destroyOnOwnTurnStart = true;
            }

            foreach (Minion mnn in p.enemyMinions)
            {
                mnn.destroyOnOwnTurnStart = true;
            }

        }

    }

}