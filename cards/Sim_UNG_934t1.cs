using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_934t1 : SimTemplate //Sulfuras
    {

        //Battlecry: Your Hero Power becomes 'Deal 8 damage to a random enemy.'

        CardDB.Card dieinsect = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.UNG_934t2); // DIE, INSECT!
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (ownplay)
            {
                p.ownHeroAblility.card = dieinsect;
                p.ownAbilityReady = true;
                p.heroPowerActivationsThisTurn = 0;
            }
        }

    }

}