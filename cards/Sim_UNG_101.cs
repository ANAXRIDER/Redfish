using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_101 : SimTemplate //Shellshifter
    {

        //Choose One - Transforminto a 5/3 with Stealth;or a 3/5 with Taunt.

        CardDB.Card Stealth53 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_101t);// 5/3stealth
        CardDB.Card taunt35 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_101t2);// 3/5 tunt minion.
        CardDB.Card StealthTaunt55 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_101t3);// 5/5 minion.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (hasfandral && own.own)
            {
                p.minionTransform(own, StealthTaunt55);
            }
            else if (choice == 1)
            {
                p.minionTransform(own, Stealth53);
            }
            else if (choice == 2)
            {
                p.minionTransform(own, taunt35);
            }
        }
    }

}