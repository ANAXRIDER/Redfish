using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_KAR_063 : SimTemplate //Spirit Claws
    {
        // Has +2 Attack while you have Spell Damage.

        CardDB.Card card = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.KAR_063);

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.equipWeapon(card, ownplay);

            List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
            bool hasspellpower = false;
            foreach (Minion m in temp)
            {
                //if we have allready a mechanical, we are buffed
                if (m.spellpower >= 1) hasspellpower = true;
            }
            if (p.spellpower >= 1) hasspellpower = true;

            if (hasspellpower)
            {
                if (ownplay)
                {
                    p.ownWeaponAttack += 2;
                    p.minionGetBuffed(p.ownHero, 2, 0);
                }
                else
                {
                    p.enemyWeaponAttack += 2;
                    p.minionGetBuffed(p.enemyHero, 2, 0);
                }
            }
        }



    }
}