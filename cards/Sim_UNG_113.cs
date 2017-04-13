using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_113 : SimTemplate //Bright-Eyed Scout
    {

        //Battlecry: Draw a card. Change its Cost to (5).

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            p.drawACard(CardDB.cardIDEnum.None, ownplay);
        }

    }

}