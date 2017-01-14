using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Pen_EX1_066 : PenTemplate //acidicswampooze
    {
        public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
        {
            if (p.enemyHeroName == HeroEnum.warrior || p.enemyHeroName == HeroEnum.thief || p.enemyHeroName == HeroEnum.pala || p.enemyHeroName == HeroEnum.shaman || p.enemyHeroName == HeroEnum.hunter)
            {
                bool canPlayAnotherMob = false;
                foreach (Handmanager.Handcard hcc in p.owncards)
                {
                    if (hcc.card.type == CardDB.cardtype.MOB && hcc.card.name != CardDB.cardName.acidicswampooze && hcc.canplayCard(p))
                    {
                        canPlayAnotherMob = true;
                        break;
                    }
                }

                bool hasGoodWeapon = (p.enemyWeaponDurability > 0 && p.enemyWeaponAttack >= 1);
                if (hasGoodWeapon)
                {
                    return 0;
                }
                else
                {
                    //if (canPlayAnotherMob) return 10;
                    return 26 - 2 * p.ownMaxMana;             
                }
            }

            return 0;
        }
	}
}
