using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_160 : SimTemplate //* powerofthewild
    {
        //Choose One - Give your minions +1/+1; or Summon a 3/2 Panther.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_160t);//panther

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (hasfandral && ownplay)
            {
                int pos = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
                p.callKid(kid, pos, ownplay, false);
                List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
                foreach (Minion m in temp)
                {
                    p.minionGetBuffed(m, 1, 1);
                }
            }
            else if (choice == 1)
            {
                List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
                foreach (Minion m in temp)
                {
                    p.minionGetBuffed(m, 1, 1);
                }
            }
            else if (choice == 2)
            {
                int pos = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
                p.callKid(kid, pos, ownplay, false);

            }
        }
    }
}