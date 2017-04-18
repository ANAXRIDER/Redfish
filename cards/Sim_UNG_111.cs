using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_111 : SimTemplate //Living Mana
    {

        //Transform your Mana Crystals into 2/2 minions. Recover the mana when they die.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_111t1); //Mana Treant

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<Minion> temp = (ownplay) ? p.ownMinions : p.enemyMinions;
            int summoncount = 7 - temp.Count;

            for (int i = 1; i <= summoncount; i++)
            {
                if (p.ownMaxMana == 0) break;
                p.callKid(kid, i, ownplay);
                if (ownplay) p.ownMaxMana--;
                else p.enemyMaxMana--;
            }
        }

    }

}