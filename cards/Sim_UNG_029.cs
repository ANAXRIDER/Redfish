using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_029 : SimTemplate //Shadow Visions
    {

        //Discover a copy of a spell in your deck.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (ownplay)
            {
                CardDB.Card c;
                int count = 0;
                foreach (KeyValuePair<CardDB.cardIDEnum, int> cid in Hrtprozis.Instance.turnDeck)
                {
                    c = CardDB.Instance.getCardDataFromID(cid.Key);
                    if (c.type == CardDB.cardtype.SPELL)
                    {
                        for (int i = 0; i < cid.Value; i++)
                        {
                            if (count > 0) break;
                            p.CardToHand(c.name, true);
                            count++;
                        }
                        if (count > 0) break;
                    }
                }
            }
        }

    }

}