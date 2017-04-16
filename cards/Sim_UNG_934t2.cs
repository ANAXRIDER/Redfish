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
            if (ownplay)
            {
                int count = (ownplay) ? p.enemyMinions.Count : p.ownMinions.Count;
                int fatiguedamage = (p.enemyDeckSize == 0) ? p.enemyHeroFatigue + 1 : 0;

                bool loggtarget = false;
                if (loggtarget) Helpfunctions.Instance.logg("@@@@@T A R G E T@@@@@");
                //Helpfunctions.Instance.logg("fatiguedamage= " + fatiguedamage); 
                if (count >= 1 && p.enemyHero.Hp + p.enemyHero.armor - fatiguedamage >= 9)
                {
                    List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                    temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                                                               //if (turnEndOfOwner == true)
                                                               //{
                                                               //    temp2.Reverse();//damage the highest if mine
                                                               //}
                    int playablecardnum = 0;
                    foreach (Handmanager.Handcard hcc in p.owncards)
                    {
                        if (hcc.getManaCost(p) <= p.manaTurnEnd && hcc.card.name != CardDB.cardName.dieinsect) playablecardnum++;
                    }

                    if (p.isEnemyHasLethal() && playablecardnum == 0)
                    {
                        temp2.Sort((a, b) => b.Angr.CompareTo(a.Angr));//damage the Highest Angr
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
                        foreach (Minion mins in temp2)
                        {
                            if (mins.Hp >= 9 || mins.divineshild) continue;
                            p.minionGetDamageOrHeal(mins, 8);
                            //if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
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
                    int attackablecardnum = 0;
                    foreach (Minion mnn in p.ownMinions)
                    {
                        if (mnn.Angr >= 1 && mnn.Ready) attackablecardnum++;
                    }
                    bool cankillandkilled = false;
                    foreach (Minion mnn in p.enemyMinions)
                    {
                        if (p.ownWeaponAttack >= mnn.Hp && p.ownHero.Hp > mnn.Angr && !p.ownHero.frozen)
                        {
                            //if () cankillbyherocount++;
                            if (p.playactions.Find(a => a.actionType == actionEnum.attackWithHero && a.target.entityID == mnn.entityID) != null)
                            {
                                cankillandkilled = true;
                                //Helpfunctions.Instance.logg("cankillbyherocount= " + cankillandkilled);
                            }

                        }
                    }

                    if (attackablecardnum == 0)// && ((cankillandkilled && p.enemyMinions.Count <= 1) || (count == 1 && p.ownHero.Hp >= 10) || (!cankillandkilled && p.enemyMinions.Count >= 3)))
                    {
                        if (ownplay == true) // if mine
                        {
                            if (!p.enemyHero.immune)
                            {
                                p.minionGetDamageOrHeal(ownplay ? p.enemyHero : p.ownHero, 8);
                                p.evaluatePenality += 10000 - (8 - p.enemyMinions.Count) * 100;
                            }
                            else
                            {
                                List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                                temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                                temp2.Reverse();//damage the highest if mine
                                foreach (Minion mins in temp2)
                                {
                                    p.minionGetDamageOrHeal(mins, 8);
                                    if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                                    break;
                                }
                            }
                        }
                        else // if not mine, hits lowest one of mines.
                        {
                            List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                            temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                            foreach (Minion mins in temp2)
                            {
                                p.minionGetDamageOrHeal(mins, 8);
                                if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                                break;
                            }
                        }

                    }
                    else
                    {
                        List<Minion> temp2 = (ownplay) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                        temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                        if (ownplay == true)
                        {
                            temp2.Reverse();//damage the highest if mine
                        }
                        foreach (Minion mins in temp2)
                        {
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