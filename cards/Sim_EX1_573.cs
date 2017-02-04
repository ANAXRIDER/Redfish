using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_573 : SimTemplate //* Cenarius
    {
        //Choose One - Give your other minions +2/+2; or Summon two 2/2 Treants with Taunt.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_573t); //special treant

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;

            if (hasfandral && own.own)
            {
                p.callKid(kid, own.zonepos - 1, own.own);
                p.callKid(kid, own.zonepos, own.own);
                own.zonepos++; 
                List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
                foreach (Minion m in temp)
                {
                    if (own.entityID != m.entityID) p.minionGetBuffed(m, 2, 2);
                }
            }
            else if (choice == 1)
            {
                List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
                foreach (Minion m in temp)
                {
                    if (own.entityID != m.entityID) p.minionGetBuffed(m, 2, 2);
                }
            }
            else if (choice == 2)
            {
                p.callKid(kid, own.zonepos - 1, own.own);
                p.callKid(kid, own.zonepos, own.own);
                own.zonepos++;
            }

        }
    }
}