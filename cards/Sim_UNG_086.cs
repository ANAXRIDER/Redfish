using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_086 : SimTemplate //Giant Anaconda
    {

        //Deathrattle: Summon a minion from your hand with 5 or more Attack.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_028);//Stranglethorn Tiger

        public override void onDeathrattle(Playfield p, Minion m)
        {
            p.callKid(kid, m.zonepos - 1, m.own);
        }

    }

}