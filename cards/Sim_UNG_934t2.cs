using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_934t2 : SimTemplate //DIE, INSECT!
    {

        //Hero PowerDeal 8 damage to a random enemy.

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
            if (ownplay)
            {
                int count = (ownplay) ? p.enemyMinions.Count : p.ownMinions.Count;
                int fatiguedamage = (p.enemyDeckSize == 0) ? p.enemyHeroFatigue + 1 : 0;

                bool loggtarget = false;
                //if (loggtarget) Helpfunctions.Instance.logg("@@@@@T A R G E T@@@@@");
                //Helpfunctions.Instance.logg("fatiguedamage= " + fatiguedamage); 
                if (count >= 1 && p.enemyHero.Hp + p.enemyHero.armor - fatiguedamage >= 9)
                {
                    
                    temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest hp

                    int playablecardnum = 0;
                    foreach (Handmanager.Handcard hcc in p.owncards)
                    {
                        if (hcc.getManaCost(p) <= p.manaTurnEnd && hcc.card.name != CardDB.cardName.dieinsect) playablecardnum++;
                    }

                    if (p.isEnemyHasLethal() && playablecardnum == 0)
                    {
                        temp2.Sort((a, b) => b.Angr.CompareTo(a.Angr));//Highest Angr to lowest
                        foreach (Minion mins in temp2)
                        {
                            if (mins.Hp >= 9 || mins.divineshild) continue;
                            p.minionGetDamageOrHeal(mins, 8);
                            if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                            break;
                        }
                    }
                    else
                    {
                        temp2.Sort((a, b) => a.Angr.CompareTo(b.Angr));//Lowest Angr to Highest
                        int i = 1;
                        foreach (Minion mins in temp2)
                        {
                            i++;
                            if (i <= temp2.Count / 2 + 1) continue; //damage to middle-high
                            p.minionGetDamageOrHeal(mins, 8); 
                            if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                            break;
                        }
                    }

                }
                else if (count == 0)
                {
                    p.minionGetDamageOrHeal(ownplay ? p.enemyHero : p.ownHero, 8);
                    if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T /// IS HERO");
                }
                else //chance to lethal
                {
                    if (!p.enemyHero.immune)
                    {
                        p.minionGetDamageOrHeal(ownplay ? p.enemyHero : p.ownHero, 8);
                        p.evaluatePenality += 10000 - (8 - p.enemyMinions.Count) * 100;
                    }
                    else
                    {
                        temp2.Sort((a, b) => a.Angr.CompareTo(b.Angr));//Lowest Angr to Highest
                        int i = 1;
                        foreach (Minion mins in temp2)
                        {
                            i++;
                            if (i <= temp2.Count / 2 + 1) continue; //damage to middle-high
                            p.minionGetDamageOrHeal(mins, 8);
                            if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                            break;
                        }
                    }
                }
            }
        }
    


    }

}