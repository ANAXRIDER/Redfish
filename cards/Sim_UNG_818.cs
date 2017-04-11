using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_818 : SimTemplate //Volatile Elemental
    {

        //Deathrattle: Deal 3 damage to a random enemy minion.

        public override void onDeathrattle(Playfield p, Minion m)
        {
            Minion target = (m.own) ? p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchLowestHP) : p.searchRandomMinion(p.ownMinions, Playfield.searchmode.searchLowestHP);
            //Helpfunctions.Instance.ErrorLog("target = " + target.entityID);
            p.minionGetDamageOrHeal(target, 3);
        }

    }

}