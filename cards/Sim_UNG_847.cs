using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_847 : SimTemplate //Blazecaller
    {

        //Battlecry: If you played an_Elemental last turn, deal 5 damage.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (true) // an_Elemental last turn require
            {
                if (target != null) //
                {
                    int dmg = 5;
                    p.minionGetDamageOrHeal(target, dmg);
                }
            }
        }

    }

}