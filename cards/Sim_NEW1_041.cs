using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_NEW1_041 : SimTemplate//Stampeding Kodo
    {
        //kill priority

        //taunt -> lowest hp -> stealth
        
        //todo list
        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            List<Minion> temp2 = (own.own) ? new List<Minion>(p.enemyMinions) : new List<Minion>(p.ownMinions);
            List<Minion> under2taunt = new List<Minion>();
            List<Minion> under2stealth = new List<Minion>();


            foreach (Minion enemy in temp2)
            {
                if (enemy.Angr <= 2 && enemy.taunt)
                {
                    under2taunt.Add(enemy);
                }
                if (enemy.Angr <= 2 && enemy.stealth)
                {
                    under2stealth.Add(enemy);
                }
            }

            if (under2taunt.Count >= 1) //taunt first
            {
                under2taunt.Sort((a, b) => a.Hp.CompareTo(b.Hp));//destroys the weakest hp
                foreach (Minion enemy in under2taunt)
                {
                    if (enemy.Angr <= 2)
                    {
                        p.minionGetDestroyed(enemy);
                        break;
                    }
                }
            }
            else 
            {
                if (under2stealth.Count == 0)
                {
                    temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//destroys the weakest hp
                    foreach (Minion enemy in temp2)
                    {
                        if (enemy.Angr <= 2)
                        {
                            p.minionGetDestroyed(enemy);
                            break;
                        }
                    }
                }
                else if (under2stealth.Count >= 1)
                {
                    if (under2stealth.Count < temp2.Count) // exist non-stealth target
                    {
                        temp2.Sort((a, b) => a.Hp.CompareTo(b.Hp));//destroys the weakest hp
                        foreach (Minion enemy in temp2)
                        {
                            if (enemy.Angr <= 2 && !enemy.stealth)
                            {
                                p.minionGetDestroyed(enemy);
                                break;
                            }
                        }
                    }
                    else if (under2stealth.Count == temp2.Count) // all targets are stealth!
                    {
                        under2stealth.Sort((b, a) => (a.Hp + a.Angr * 2).CompareTo(b.Hp + b.Angr * 2));//destroys the strongest hp
                        foreach (Minion enemy in under2stealth)
                        {
                            if (enemy.Angr <= 2)
                            {
                                p.minionGetDestroyed(enemy);
                                break;
                            }
                        }
                    }
                }               
            }         
        }
    }
}
