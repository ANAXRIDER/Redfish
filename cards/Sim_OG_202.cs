using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_OG_202 : SimTemplate //* Mire Keeper
    {
        //Choose One - Summon a 2/2 Slime; or Gain an empty Mana Crystal.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NAX11_03);

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && own.own))
            {
                int pos = (own.own) ? p.ownMinions.Count : p.enemyMinions.Count;
                p.callKid(kid, pos, own.own);
            }
            if (choice == 2 || (hasfandral && own.own))
            {
                if (own.own) p.ownMaxMana = Math.Min(10, p.ownMaxMana + 1);
                else p.enemyMaxMana = Math.Min(10, p.enemyMaxMana + 1);
            }
        }
    }
}