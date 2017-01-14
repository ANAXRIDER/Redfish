using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Pen_OG_114 : PenTemplate //* Forbidden Ritual
    {
        //Spend all your Mana. Summon that many 1/1 Tentacles.

        public override float getPlayPenalty(Playfield p, Handmanager.Handcard hc, Minion target, int choice, bool isLethal)
        {
            if (p.mana == 0 || p.ownMinions.Count == 7) return 500;
            else
            {
                int pos = p.ownMinions.Count;
                int anz = Math.Min(7 - pos, p.mana);

                int pen = (15 / anz); //penalize low mana
                
                bool hassynergyonboard = false;
                bool hassynergyinhand = false;
                bool hasseaGiant = false;
                foreach (Minion mnn in p.ownMinions)
                {
                    if ((mnn.name == CardDB.cardName.knifejuggler && !mnn.silenced) || (mnn.name == CardDB.cardName.darkshirecouncilman && !mnn.silenced)) hassynergyonboard = true;
                }
                foreach (Handmanager.Handcard hcc in p.owncards)
                {
                    if (hcc.card.name == CardDB.cardName.knifejuggler || hcc.card.name == CardDB.cardName.darkshirecouncilman) hassynergyinhand = true;
                    if (hcc.card.name == CardDB.cardName.seagiant) hasseaGiant = true;
                }
                if (hassynergyinhand && !hassynergyonboard) pen += 10;
                //if (!hassynergyonboard ) pen += 3;
                if (hassynergyonboard) pen -= anz;

                if (hasseaGiant && !hassynergyonboard) pen -= anz;
                //Helpfunctions.Instance.ErrorLog("ritual pen" + pen);
                return pen;
            }
        }
    }
}