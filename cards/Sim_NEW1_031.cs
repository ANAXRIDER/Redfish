using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_NEW1_031 : SimTemplate //animalcompanion
	{

        //    ruft einen zufälligen wildtierbegleiter herbei.
        CardDB.Card c1 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NEW1_034);//Huffer
        CardDB.Card c2 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NEW1_033);//Leokk
        CardDB.Card c3 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NEW1_032);//Misha

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
		{
            int placeoffather = (ownplay)?  p.ownMinions.Count : p.enemyMinions.Count;
            if (p.ownMaxMana <= 2) p.callKid(c2, placeoffather, ownplay); //Leokk
            else if (p.enemyMinions.Count == 0 || p.ownMinions.Count >= 4) p.callKid(c2, placeoffather, ownplay); //Leokk
            else p.callKid(c3, placeoffather, ownplay); //Misha
        }

	}
}