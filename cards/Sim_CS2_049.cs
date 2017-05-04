using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_CS2_049 : SimTemplate //totemiccall
    {
        //    Summon a random basic totem.
        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_050);// searing
        CardDB.Card kid2 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_052);//spellpower
        CardDB.Card kid3heal = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NEW1_009);//
        CardDB.Card kid4taunt = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_051);//
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<CardDB.cardIDEnum> availa = new List<CardDB.cardIDEnum>
            {
                CardDB.cardIDEnum.CS2_052,
                CardDB.cardIDEnum.CS2_051,
                CardDB.cardIDEnum.NEW1_009,
                CardDB.cardIDEnum.CS2_050
            };

            foreach (Minion m in (ownplay) ? p.ownMinions : p.enemyMinions)
                {
                    if (availa.Contains(m.handcard.card.cardIDenum))
                    {
                        availa.Remove(m.handcard.card.cardIDenum);
                    }
                }

            int posi = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
            /*bool spawnspellpower = true;
            foreach (Minion m in (ownplay) ? p.ownMinions : p.enemyMinions)
            {
                if (m.handcard.card.cardIDenum == CardDB.cardIDEnum.CS2_052)
                {
                    spawnspellpower = false;
                    break;
                }
            }
            p.callKid((spawnspellpower) ? kid2 : kid, posi, ownplay);*/

            if (availa.Contains(CardDB.cardIDEnum.CS2_051) && p.isEnemyHasLethal() <= 0 && (p.ownHero.numAttacksThisTurn == 0 || p.ownMinions.Find(a => a.allreadyAttacked) == null)) // taunt when enemy has lethal
            {
                p.callKid(kid4taunt, posi, ownplay);
                return;
            }

            if (availa.Contains(CardDB.cardIDEnum.CS2_050)) //always 1/1totem first
            {
                p.callKid(kid, posi, ownplay);
                return;
            }

            if (availa.Contains( CardDB.cardIDEnum.CS2_052)) //spellpower 1/3chance is enough 
            {
                p.callKid(kid2, posi, ownplay);
                return;
            }

            if (availa.Contains(CardDB.cardIDEnum.NEW1_009)) // heal 1/2
            {
                p.callKid(kid3heal, posi, ownplay);
                return;
            }

            if (availa.Contains( CardDB.cardIDEnum.CS2_051))
            {
                p.callKid(kid4taunt, posi, ownplay);
                
                return;
            }

            
        }
    }

}