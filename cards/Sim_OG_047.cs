using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_OG_047 : SimTemplate //* Feral Rage
    {
        //Choose One - Give your hero +4 attack this turn; or Gain 8 armor.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && ownplay))
            {
                if (ownplay) p.minionGetTempBuff(p.ownHero, 4, 0);
                else p.minionGetTempBuff(p.enemyHero, 4, 0);
            }
            if (choice == 2 || (hasfandral && ownplay))
            {
                if (ownplay) p.minionGetArmor(p.ownHero, 8);
                else p.minionGetArmor(p.enemyHero, 8);
            }
        }
    }
}