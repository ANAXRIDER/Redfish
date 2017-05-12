using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_UNG_089 : SimTemplate //Gentle Megasaur
    {

        //Battlecry: Adapt your_Murlocs.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            List<Minion> temp = (own.own) ? p.ownMinions : p.enemyMinions;
            bool hasreadymurloc = false;
            if (p.ownMinions.Find(a => (TAG_RACE)a.handcard.card.race == TAG_RACE.MURLOC && a.Ready) != null)
            {
                hasreadymurloc = true;
            }

            bool first = true;
            int bestAdapt = 0;
            foreach (Minion m in temp)
            {
                if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
                {
                    if (first)
                    {
                        bestAdapt = p.getBestAdapt(m, hasreadymurloc);
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
                            if ((TAG_RACE)m.handcard.card.race == TAG_RACE.MURLOC)
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
