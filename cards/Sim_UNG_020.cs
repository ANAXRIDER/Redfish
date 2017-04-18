using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_020 : SimTemplate //* Arcanologist
    {
        //Battlecry: Draw a Secret from your deck.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            if (own.own)
            {
                CardDB.Card c;
                int count = 0;
                foreach (KeyValuePair<CardDB.cardIDEnum, int> cid in Hrtprozis.Instance.turnDeck)
                {
                    c = CardDB.Instance.getCardDataFromID(cid.Key);
                    if (c.Secret)
                    {
                        for (int i = 0; i < cid.Value; i++)
                        {
                            if (count > 0) break;
                            p.drawACard(c.name, true);
                            count++;
                        }
                        if (count > 0) break;
                    }
                }
            }
        }
    }
}