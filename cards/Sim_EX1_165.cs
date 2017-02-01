using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_165 : SimTemplate  //* Druid of the Claw
    {
        // Choose One - Charge; or +2 Health and Taunt.

        CardDB.Card cat = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_165t1);
        CardDB.Card bear = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_165t2);
        CardDB.Card bearcat = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.OG_044a);

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (hasfandral && own.own)
            {
                p.minionTransform(own, bearcat);
            }
            else if (choice == 1)
            {
                p.minionTransform(own, cat);
            }
            else if (choice == 2)
            {
                p.minionTransform(own, bear);
            }
        }
    }
}