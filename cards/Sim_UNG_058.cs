using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_058 : SimTemplate //Razorpetal Lasher
    {

        //Battlecry: Add aRazorpetal to your handthat deals 1 damage.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            p.CardToHand(CardDB.cardName.razorpetal, own.own);
        }

    }

}