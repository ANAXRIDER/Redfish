using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_KAR_114 : SimTemplate //Barnes
    {
        // Battlecry: Summon a 1/1 copy of a random minion in your deck.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.AT_058); // kings elekk (for a conservative proc)

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            //int pos = (own.own) ? own.zonepos - 1 : p.enemyMinions.Count;
            //p.callKid(kid, pos, own.own);

            //List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
            //temp[pos].Hp = 1;
            //temp[pos].Angr = 1;





            int count = (own.own) ? p.ownMinions.Count : p.enemyMinions.Count;
            p.callKid(kid, own.zonepos, own.own, true);

            if (count < 6)
            {
                List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
                int pos = Math.Min(own.zonepos - 1, temp.Count - 1);
                Minion kidminion = temp[pos]; //barnes isn't on the playfield yet but the kid is where barnes will be

                int angr = 1 - kidminion.Angr;
                int hp = 1 - kidminion.maxHp;

                p.minionGetBuffed(kidminion, angr, hp);
            }



        }
    }
}