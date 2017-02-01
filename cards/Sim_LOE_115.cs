using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_LOE_115 : SimTemplate //* Raven Idol
    {
        //Choose one - Discover a minion; or Discover a spell.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && ownplay))
            {
                p.drawACard(CardDB.cardName.unknown, ownplay, true);
            }
            if (choice == 2 || (hasfandral && ownplay))
            {
                p.drawACard(CardDB.cardName.unknown, ownplay, true);
            }
        }
    }
}
