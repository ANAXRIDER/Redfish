using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_085 : SimTemplate //Emerald Hive Queen
    {

        //Your minions cost (2) more.

        public override void onAuraStarts(Playfield p, Minion own)
        {
            if (own.own) p.anzEmeraldHiveQueen++;
        }

        public override void onAuraEnds(Playfield p, Minion own)
        {
            if (own.own) p.anzEmeraldHiveQueen--;
        }

    }

}