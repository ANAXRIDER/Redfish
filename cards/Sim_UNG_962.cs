using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_962 : SimTemplate //Lightfused Stegodon
    {

        //Battlecry: Adapt your Silver_Hand Recruits.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
            bool hasreadyminion = false;
            if (p.ownMinions.Find(a => a.Ready && a.name == CardDB.cardName.silverhandrecruit) != null)
            {
                hasreadyminion = true;
            }
            bool first = true;
            int bestAdapt = 0;
            foreach (Minion m in temp)
            {
                if (m.name == CardDB.cardName.silverhandrecruit)
                {
                    if (first)
                    {
                        bestAdapt = p.getBestAdapt(m , hasreadyminion);
                        first = false;
                    }
                    else
                    {
                        switch (bestAdapt)
                        {
                            case 1: p.minionGetBuffed(m, 3, 0); break;
                            case 2: p.minionGetBuffed(m, 1, 1); break;
                            case 3: m.taunt = true; break;
                            case 4: m.divineshild = true; break;
                            case 5: m.poisonous = true; break;
                        }
                    }
                }
            }
        }





        public override void onAdaptChoice(Playfield p, bool ownplay, Minion target, CardDB.cardIDEnum choice)
        {
            List<Minion> temp = p.ownMinions;
            switch (choice)
            {
                case CardDB.cardIDEnum.UNG_999t3:  // 3attack
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                p.minionGetBuffed(m, 3, 0);
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t8://divine shield
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.divineshild = true;
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t13: //poisonous
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.poisonous = true;
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t14:// +1/+1
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                p.minionGetBuffed(m, 1, 1);
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t4:// 3hp
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                p.minionGetBuffed(m, 0, 3);
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t2://deathrattle 2 1/1 plants
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.livingspores++;
                            }
                        }
                        break;
                    }

                case CardDB.cardIDEnum.UNG_999t7: //windfury
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                p.minionGetWindfurry(m);
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t6: //taunt
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.taunt = true;
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t10://stealth
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.stealth = true;
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                case CardDB.cardIDEnum.UNG_999t5://can't be targeted
                    {
                        foreach (Minion m in temp)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                m.AdaptedCantBeTargetedBySpellsOrHeroPowers++;
                                //Helpfunctions.Instance.ErrorLog("working");
                            }
                        }
                        break;
                    }
                default: break;
            }


        }





    }

}