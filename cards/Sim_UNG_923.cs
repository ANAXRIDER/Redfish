using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_923 : SimTemplate //Iron Hide
    {

        //Gain 5 Armor.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (ownplay) p.ownHero.armor += 5;
        }

    }

}