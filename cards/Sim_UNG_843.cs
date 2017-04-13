using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_843 : SimTemplate //The Voraxx
    {

        //After you cast a spell onthis minion, summon a1/1 Plant and castanother copy on it.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_999t2t1); //Plant

        public override void onCardIsGoingToBePlayed(Playfield p, CardDB.Card c, bool wasOwnCard, Minion triggerEffectMinion, Minion target, int choice)
        {
            if (target != null && target.own == wasOwnCard && triggerEffectMinion.own == wasOwnCard && target.entityID == triggerEffectMinion.entityID && c.type == CardDB.cardtype.SPELL)
            {
                int zonepos = triggerEffectMinion.zonepos;
                p.callKid(kid, zonepos, true);

                if (p.ownMinions[zonepos].name == CardDB.cardName.plant)
                {
                    c.sim_card.onCardPlay(p, wasOwnCard, p.ownMinions[zonepos], choice);
                    p.doDmgTriggers();
                }

                
            }
        }

    }

}