using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_834 : SimTemplate //Feeding Time
    {

        //Deal $3 damage to a minion. Summon three 1/1 Pterrordaxes.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_834t1); // Pterrordax

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            int dmg = (ownplay) ? p.getSpellDamageDamage(3) : p.getEnemySpellDamageDamage(3);
            p.minionGetDamageOrHeal(target, dmg);

            int posi = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
            p.callKid(kid, posi, ownplay);
            p.callKid(kid, posi, ownplay);
            p.callKid(kid, posi, ownplay);
        }

    }

}