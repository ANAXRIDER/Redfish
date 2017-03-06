using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Pen_OG_027 : PenTemplate //* Evolve
    {
        //Transform your minions into random minions that cost (1) more.

        public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
        {
            int pen = 0;
            if (p.ownMinions.Count <= 2) pen += 10;
            return pen;
        }
    }
}