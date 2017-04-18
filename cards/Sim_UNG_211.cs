using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_211 : SimTemplate //Kalimos, Primal Lord
    {

        //Battlecry: If you played anElemental last turn, cast anElemental Invocation.

        // 1: fill with 1/1minion 
        // 2: heal own hero hp 12
        // 3: deal enemy hero hp 6
        // 4: deal 3 dmg to all enemy minion  

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (p.anzOwnElementalsLastTurn > 0 && own.own)
            { }
        }

    }

}