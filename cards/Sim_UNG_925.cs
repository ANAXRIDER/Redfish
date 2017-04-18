using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_925 : SimTemplate //Ornery Direhorn
    {

        //TauntBattlecry: Adapt.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (own.own) p.getBestAdapt(own);
        }

    }

}