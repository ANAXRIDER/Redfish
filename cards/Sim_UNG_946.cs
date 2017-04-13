using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_946 : SimTemplate //Gluttonous Ooze
    {

        //Battlecry: Destroy your opponent's weapon and gain Armor equal to its Attack.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (own.own)
            {
                p.minionGetArmor(p.ownHero, p.enemyWeaponAttack);
                p.lowerWeaponDurability(1000, false);
            }
            else
            {
                p.lowerWeaponDurability(1000, true);
            }
        }

    }

}