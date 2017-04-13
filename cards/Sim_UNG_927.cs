using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_927 : SimTemplate //Sudden Genesis
    {

        //Summon copies of your damaged minions.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
            foreach (Minion m in (ownplay) ? p.ownMinions.ToArray() : p.enemyMinions.ToArray())
            {
                if (m.wounded)
                {
                    int pos = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
                    p.callKid(m.handcard.card, pos + 1, ownplay, false);                    
                }               
            }

            int i = 0;
            foreach (Minion m in (ownplay) ? p.ownMinions.ToArray() : p.enemyMinions.ToArray())
            {                
                if (m.entityID >= 1000)
                {
                    m.setMinionTominion(temp[i]);
                    if (temp[i].charge == 0) m.Ready = false;
                    i++;
                }
            }
        }

    }

}