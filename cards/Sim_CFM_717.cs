using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_CFM_717 : SimTemplate //* Jade Claws
	{
		// Battlecry: Summon a Jade Golem. Overload: (1)

        CardDB.Card weapon = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CFM_717);

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.equipWeapon(weapon, ownplay);

            int place = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
            p.callKid(p.getNextJadeGolem(ownplay), place + 1, ownplay);

            if (p.ownMinions.Find(a => a.name == CardDB.cardName.brannbronzebeard && !a.silenced) != null)
            {
                p.callKid(p.getNextJadeGolem(ownplay), place + 2, ownplay);
            }


            p.changeRecall(ownplay, 1);
        }
    }
}