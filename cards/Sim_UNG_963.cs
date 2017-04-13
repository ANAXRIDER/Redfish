using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_963 : SimTemplate //Lyra the Sunshard
    {

        //Whenever you cast a spell, add a random Priest spell to your hand.

        public override void onCardIsGoingToBePlayed(Playfield p, CardDB.Card c, bool wasOwnCard, Minion triggerEffectMinion, Minion target, int choice)
        {
            if (triggerEffectMinion.own == wasOwnCard && c.type == CardDB.cardtype.SPELL)
            {
                p.CardToHand(CardDB.cardName.unknown, wasOwnCard);
            }
        }

    }

}