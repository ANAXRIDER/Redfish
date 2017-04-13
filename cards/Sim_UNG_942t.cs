using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_942t : SimTemplate //Megafin
    {

        //Battlecry: Fill your hand with random Murlocs.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            int original = p.owncards.Count;
            if (own.own)
            {
                for (int i = 1; i <= 10 - original; i++)
                {
                    p.CardToHand(CardDB.cardName.unknown, true);
                }
            }           
        }
    }

}