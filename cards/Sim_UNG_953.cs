using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_953 : SimTemplate //Primalfin Champion
    {

        //Deathrattle: Return any spells you cast on this minion to your hand.

        public override void onCardIsGoingToBePlayed(Playfield p, CardDB.Card c, bool wasOwnCard, Minion triggerEffectMinion, Minion target, int choice)
        {
            if (target != null)
            {
                if (triggerEffectMinion.entityID == target.entityID && triggerEffectMinion.own == wasOwnCard && c.type == CardDB.cardtype.SPELL)
                {
                    triggerEffectMinion.ReturnSpellCount++;
                }
            }           
        }

        public override void onDeathrattle(Playfield p, Minion m)
        {
            for (int i = 1; i <= m.ReturnSpellCount; i++)
            {
                p.CardToHand(CardDB.cardName.unknown, m.own);
            }            
        }

    }

}