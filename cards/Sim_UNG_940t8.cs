using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_940t8 : SimTemplate //Amara, Warden of Hope
    {

        //Taunt Battlecry: Set yourhero's Health to 40.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            p.minionGetDamageOrHeal(p.ownHero, p.ownHero.Hp - p.ownHero.maxHp, true);//fully heal //like reno... 

            if (own.own) p.ownHero.Hp = 40;
            else p.enemyHero.Hp = 40;
        }

    }

}