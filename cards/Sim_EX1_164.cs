using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_164 : SimTemplate //* nourish
    {

        //    Choose One - Gain 2 Mana Crystals or Draw 3 cards.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && ownplay))
            {
                if (ownplay)
                {
                    p.mana = Math.Min(10, p.mana + 2);
                    p.ownMaxMana = Math.Min(10, p.ownMaxMana + 2);
                }
                else
                {
                    p.mana = Math.Min(10, p.mana + 2);
                    p.enemyMaxMana = Math.Min(10, p.enemyMaxMana + 2);
                }
            }
            if (choice == 2 || (hasfandral && ownplay))
            {
                //this.owncarddraw+=3;
                p.drawACard(CardDB.cardName.unknown, ownplay);
                p.drawACard(CardDB.cardName.unknown, ownplay);
                p.drawACard(CardDB.cardName.unknown, ownplay);
            }
        }
    }
}