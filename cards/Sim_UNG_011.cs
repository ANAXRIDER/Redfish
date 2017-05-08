using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_011 : SimTemplate //* Hydrologist
    {
        //Battlecry: Discover a Secret.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            p.CardToHand(CardDB.cardName.noblesacrifice, own.own); //assume always pick sacrifice
        }
    }
}