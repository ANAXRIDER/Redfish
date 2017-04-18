using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_836 : SimTemplate //Clutchmother Zavas
    {

        //Whenever you discard this, give it +2/+2 and return it to your hand.

        public override void onCardIsDiscarded(Playfield p, CardDB.Card card, bool own)
        {
            if (own)
            {
                p.CardToHand(CardDB.cardName.clutchmotherzavas, own);

                Handmanager.Handcard hc = p.owncards[p.owncards.Count - 1];
                if (hc != null)
                {
                    hc.addattack += 2;
                    hc.addHp += 2;
                    p.anzOwnExtraAngrHp += 4;
                }
                foreach (Handmanager.Handcard hccc in p.owncards)
                {
                    Helpfunctions.Instance.ErrorLog("" + hccc.card.name);
                    Helpfunctions.Instance.ErrorLog("addattack : " + hccc.addattack + "addhp : " + hccc.addHp);
                }
                
            }
            else p.enemyAnzCards++;
        }

    }

}