using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_948 : SimTemplate //Molten Reflection
    {

        //Choose a friendly minion. Summon a copy of it.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (target != null)
            {
                p.callKid(target.handcard.card, p.ownMinions.Count, ownplay);
                List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
                foreach (Minion mnn in temp)
                {
                    if (mnn.name == target.name && target.entityID != mnn.entityID)
                    {
                        mnn.setMinionTominion(target);
                        if (target.charge == 0) mnn.Ready = false;
                        break;
                    }
                }
            }

        }

    }

}