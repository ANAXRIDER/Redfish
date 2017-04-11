using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_917 : SimTemplate //Dinomancy
    {

        //Your Hero Power becomes 'Give a Beast +2/+2.'

        CardDB.Card hp = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_917t1);//hunter

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.ownHeroAblility = new Handmanager.Handcard(hp);
            p.ownAbilityReady = true;
        }

    }

}