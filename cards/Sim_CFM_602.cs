using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_CFM_602 : SimTemplate //* Jade Idol
	{
		// Choose One - Summon a Jade Golem; or Shuffle 3 copies of this card into your deck.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            bool hasfandral = false;
            if (p.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null) hasfandral = true;
            if (choice == 1 || (hasfandral && ownplay))
            {
                int pos = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
                p.callKid(p.getNextJadeGolem(ownplay), pos, ownplay);
            }
            if (choice == 2 || (hasfandral && ownplay))
            {
                if (ownplay)
                {
                    //Hrtprozis.Instance.AddTurnDeck(BoardTester.Instance.td, CardDB.cardIDEnum.CFM_602, 3);
                    p.ownDeckSize += 3;
                    //if (p.ownHeroName == HeroEnum.druid && p.anzOwnJadeGolem >= 2) p.evaluatePenality -= 11;
                }
                else p.enemyDeckSize += 3;
            }
        }
    }
}