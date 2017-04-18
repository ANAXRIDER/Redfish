using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_028t : SimTemplate //Time Warp
    {

        //Take an extra turn.
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (p.ownMaxMana <= 9) p.ownMaxMana++;
            p.mana = p.ownMaxMana;
            p.triggerStartTurn(true);
        }
    }

}