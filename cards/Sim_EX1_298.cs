using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_EX1_298 : SimTemplate //ragnarosthefirelord
    {
        public override void onTurnEndsTrigger(Playfield p, Minion triggerEffectMinion, bool turnEndOfOwner)
        {
            if (triggerEffectMinion.own == turnEndOfOwner)
            {
                int count = (turnEndOfOwner) ? p.enemyMinions.Count : p.ownMinions.Count;
                int fatiguedamage = (p.enemyDeckSize == 0) ? p.enemyHeroFatigue + 1 : 0;

                bool loggtarget = false;
                if (loggtarget) Helpfunctions.Instance.logg("@@@@@T A R G E T@@@@@");
                //Helpfunctions.Instance.logg("fatiguedamage= " + fatiguedamage); 
                if (count >= 1 && p.enemyHero.Hp + p.enemyHero.armor - fatiguedamage >= 9)
                {
                    List<Minion> temp2 = (turnEndOfOwner) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                    temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                    //if (turnEndOfOwner == true)
                    //{
                    //    temp2.Reverse();//damage the highest if mine
                    //}
                    
                    foreach (Minion mins in temp2)
                    {
                        if (mins.Hp >= 9 || mins.divineshild) continue;
                        p.minionGetDamageOrHeal(mins, 8);
                        if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T ///" + mins.name);
                        break;
                    }
                }
                else if (count == 0)
                {
                    p.minionGetDamageOrHeal(turnEndOfOwner ? p.enemyHero : p.ownHero, 8);
                    if (loggtarget) Helpfunctions.Instance.logg("/// T A R G E T /// IS HERO" );
                }
                else //chance to lethal
                {
                    int playablecardnum = 0;
                    foreach (Handmanager.Handcard hcc in p.owncards)
                    {
                        if (hcc.getManaCost(p) <= p.manaTurnEnd) playablecardnum++;
                    }
                    if (p.ownHeroAblility.card.getManaCost(p, 2) <= p.manaTurnEnd && p.ownAbilityReady) playablecardnum++;
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
                                       
                    if (playablecardnum == 0 && attackablecardnum == 0)// && ((cankillandkilled && p.enemyMinions.Count <= 1) || (count == 1 && p.ownHero.Hp >= 10) || (!cankillandkilled && p.enemyMinions.Count >= 3)))
                    {
                        if (turnEndOfOwner == true) // if mine
                        {
                            if (!p.enemyHero.immune) p.minionGetDamageOrHeal(turnEndOfOwner ? p.enemyHero : p.ownHero, 8);
                            else
                            {
                                List<Minion> temp2 = (turnEndOfOwner) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
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
                            List<Minion> temp2 = (turnEndOfOwner) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
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
                        List<Minion> temp2 = (turnEndOfOwner) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
                        temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//damage the lowest
                        if (turnEndOfOwner == true)
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
                triggerEffectMinion.stealth = false;
            }
        }
    }
}