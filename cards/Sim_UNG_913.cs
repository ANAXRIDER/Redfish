using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_913 : SimTemplate //Tol'vir Warden
    {

        //Battlecry: Draw two 1-Cost minions from your deck.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (own.own)
            {
                CardDB.Card c;
                int count = 0;
                foreach (KeyValuePair<CardDB.cardIDEnum, int> cid in Hrtprozis.Instance.turnDeck)
                {
                    c = CardDB.Instance.getCardDataFromID(cid.Key);
                    if (c.cost == 1 && c.type == CardDB.cardtype.MOB)
                    {
                        for (int i = 0; i < cid.Value; i++)
                        {
                            p.drawACard(c.name, true);
                            count++;
                            if (count > 1) break;
                        }
                        if (count > 1) break;
                    }
                }
            }
        }

    }

}