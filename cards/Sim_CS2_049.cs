using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_CS2_049 : SimTemplate //totemiccall
    {
        //    Summon a random basic totem.
        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_050);// searing
        CardDB.Card kid2 = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_052);//spellpower
        CardDB.Card kid3heal = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.NEW1_009);//
        CardDB.Card kid4taunt = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_051);//
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            List<CardDB.cardIDEnum> availa = new List<CardDB.cardIDEnum>
            {
                CardDB.cardIDEnum.CS2_052,
                CardDB.cardIDEnum.CS2_051,
                CardDB.cardIDEnum.NEW1_009,
                CardDB.cardIDEnum.CS2_050
            };

            foreach (Minion m in (ownplay) ? p.ownMinions : p.enemyMinions)
                {
                    if (availa.Contains(m.handcard.card.cardIDenum))
                    {
                        availa.Remove(m.handcard.card.cardIDenum);
                    }
                }

            int posi = (ownplay) ? p.ownMinions.Count : p.enemyMinions.Count;
            /*bool spawnspellpower = true;
            foreach (Minion m in (ownplay) ? p.ownMinions : p.enemyMinions)
            {
                if (m.handcard.card.cardIDenum == CardDB.cardIDEnum.CS2_052)
                {
                    spawnspellpower = false;
                    break;
                }
            }
            p.callKid((spawnspellpower) ? kid2 : kid, posi, ownplay);*/







            //do face-attack:
            int concedevalue = p.ownHero.Hp + p.ownHero.armor;
            int hasowntaunthp = p.ownHero.Hp + p.ownHero.armor;
            int potentialenemyattack = 0;
            int directenemyheropowerattack = 0;
            if (!p.ownHero.immune && !p.ownSecretsIDList.Contains(CardDB.cardIDEnum.EX1_295))// enemys turn ends -> attack with all minions face (if there is no taunt)
            {
                //Helpfunctions.Instance.ErrorLog(turnCounter + " bef " + p.ownHero.Hp + " " + p.ownHero.armor);
                int attack = 0;
                int directattack = 0;
                int hasowntaunt = 0;


                List<Minion> hastauntminion = new List<Minion>(p.ownMinions);
                hastauntminion.Sort((b, a) => a.Hp.CompareTo(b.Hp));//take the strongest hp
                                                                    //if (hastauntminion.Count >= 1 ) hastauntminion.Clear();

                if (p.enemyWeaponName != CardDB.cardName.foolsbane) attack += p.enemyHero.Angr;
                potentialenemyattack += p.enemyHero.Angr;
                if (p.enemyWeaponName == CardDB.cardName.doomhammer && p.enemyWeaponDurability >= 2 && hasowntaunt == 0)
                {
                    attack += p.enemyHero.Angr;
                    potentialenemyattack += p.enemyHero.Angr;
                }



                List<Minion> ownminionshp = new List<Minion>(p.ownMinions);
                List<Minion> enemyminionsattack = new List<Minion>(p.enemyMinions);

                ownminionshp.Sort((b, a) => a.Hp.CompareTo(b.Hp));//take the strongest hp
                enemyminionsattack.Sort((b, a) => a.Angr.CompareTo(b.Angr));//take the strongest angr




                foreach (Minion m in p.ownMinions)
                {
                    if (m.name == CardDB.cardName.ragnarosthefirelord)
                    {
                        List<Minion> strongest = new List<Minion>(p.enemyMinions);
                        strongest.Sort((b, a) => a.Angr.CompareTo(b.Angr));//take the strongest
                        if (strongest.Count == 0) break;
                        Minion strongestMNN = strongest[0];

                        foreach (Minion mnn in p.enemyMinions)
                        {
                            if (mnn.Angr == strongestMNN.Angr) attack -= mnn.Angr; break;
                        }
                    }
                    if (m.taunt && !m.stealth)
                    {
                        hasowntaunt++;
                        hasowntaunthp += m.Hp;
                    }
                    else
                    {
                        for (int i = 1; i <= hastauntminion.Count; i++)
                        {
                            if (hastauntminion[i - 1].name == m.name)
                            {
                                hastauntminion.RemoveAt(i - 1);
                            }
                        }
                    }
                }

                //foreach (Minion mnn in hastauntminion)
                //{
                //    if (mnn.taunt)
                //    {
                //        Helpfunctions.Instance.ErrorLog(" hasowntaunt " + mnn.name);
                //        Helpfunctions.Instance.ErrorLog(" mnn Ä«¿îÆ®" + hastauntminion.Count + "\r\n");
                //    }
                //}

                bool hasInspire2Attack = false;
                foreach (Minion m in p.enemyMinions)
                {
                    if (m.name == CardDB.cardName.ragnarosthefirelord && p.ownMinions.Count == 0 && !m.silenced)
                    {
                        attack += 8;
                        potentialenemyattack += 8;
                    }
                    if (m.Ready)
                    {
                        attack += m.Angr;
                        potentialenemyattack += m.Angr;
                    }
                    if (m.Ready && m.windfury && m.numAttacksThisTurn == 0)
                    {
                        attack += m.Angr;
                        potentialenemyattack += m.Angr;
                    }
                    if (m.name == CardDB.cardName.savagecombatant) hasInspire2Attack = true;

                }
                ////Helpfunctions.Instance.ErrorLog(" hasowntaunt " + hasowntaunt);
                //if (hasowntaunt >= 1)
                //{
                //    for (int i = 1; i <= hasowntaunt; i++)
                //    {
                //        if (i <= p.enemyMinions.Count)
                //        {
                //            attack -= enemyminionsattack[i - 1].Angr;
                //        }
                //    }
                //    if (p.enemyMinions.Count == 0 && p.enemyWeaponAttack >= 1) attack -= p.enemyWeaponAttack;
                //}
                ////Helpfunctions.Instance.ErrorLog(" attack " + attack);





                //Helpfunctions.Instance.ErrorLog(" hasowntaunt " + hasowntaunt);
                if (hasowntaunt >= 1)
                {
                    //for (int j = 1; j <= hastauntminion.Count; j++)
                    //{
                    //    if (hastauntminion[j-1].Hp == p.enemyWeaponAttack)
                    //    {
                    //        attack -= p.enemyWeaponAttack;
                    //        hastauntminion.RemoveAt(j - 1);
                    //        break;
                    //    }
                    //}
                    //Helpfunctions.Instance.ErrorLog(" hastauntminion " + hastauntminion.Count);

                    int tempEnemyweaponattack = p.enemyWeaponAttack;
                    int enemyweaponCanmultipleattack = (p.enemyWeaponName == CardDB.cardName.doomhammer && p.enemyWeaponDurability >= 2 ? 2 : 1);

                    foreach (Minion mnn in hastauntminion)
                    {
                        bool found = false;
                        for (int i = 1; i <= enemyminionsattack.Count; i++)
                        {
                            if (mnn.Hp == enemyminionsattack[i - 1].Angr || mnn.Hp == tempEnemyweaponattack)
                            {
                                found = true;
                                if (mnn.Hp == enemyminionsattack[i - 1].Angr)
                                {
                                    attack -= enemyminionsattack[i - 1].Angr;
                                    enemyminionsattack.RemoveAt(i - 1);
                                }
                                else if (mnn.Hp == tempEnemyweaponattack)
                                {
                                    attack -= tempEnemyweaponattack;
                                    if (enemyweaponCanmultipleattack == 1) tempEnemyweaponattack = 0;
                                    enemyweaponCanmultipleattack--;
                                    //hastauntminion.Remove(mnn);
                                }
                                break;
                            }
                            //else if (mnn.Hp > enemyminionsattack[i - 1].Angr || mnn.Hp > tempEnemyweaponattack)
                            //{
                            //    if (enemyminionsattack[i - 1].Angr >= tempEnemyweaponattack)
                            //    {
                            //        attack -= enemyminionsattack[Math.Max(i - 2, 0)].Angr;
                            //        enemyminionsattack.RemoveAt(Math.Max(i - 2, 0));
                            //    }
                            //    else
                            //    {
                            //        attack -= tempEnemyweaponattack;
                            //        if (enemyweaponCanmultipleattack == 1) tempEnemyweaponattack = 0;
                            //        enemyweaponCanmultipleattack--;
                            //    }

                            //    break;
                            //}
                            //else if (mnn.Hp < enemyminionsattack[i - 1].Angr || mnn.Hp < tempEnemyweaponattack)
                            //{
                            //    if (enemyminionsattack[i - 1].Angr <= tempEnemyweaponattack)
                            //    {
                            //        attack -= enemyminionsattack[enemyminionsattack.Count - 1].Angr;
                            //        enemyminionsattack.RemoveAt(enemyminionsattack.Count - 1);
                            //    }
                            //    else
                            //    {
                            //        attack -= tempEnemyweaponattack;
                            //        if (enemyweaponCanmultipleattack == 1) tempEnemyweaponattack = 0;
                            //        enemyweaponCanmultipleattack--;
                            //        //hastauntminion.Remove(mnn);
                            //    }                                  
                            //    break;
                            //}
                        }

                        if (!found)
                        {
                            for (int i = 1; i <= enemyminionsattack.Count; i++)
                            {
                                if (mnn.Hp > enemyminionsattack[i - 1].Angr || mnn.Hp > tempEnemyweaponattack)
                                {
                                    found = true;
                                    if (enemyminionsattack[i - 1].Angr >= tempEnemyweaponattack)
                                    {
                                        attack -= enemyminionsattack[Math.Max(i - 2, 0)].Angr;
                                        enemyminionsattack.RemoveAt(Math.Max(i - 2, 0));
                                    }
                                    else
                                    {
                                        attack -= tempEnemyweaponattack;
                                        if (enemyweaponCanmultipleattack == 1) tempEnemyweaponattack = 0;
                                        enemyweaponCanmultipleattack--;
                                    }
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            for (int i = 1; i <= enemyminionsattack.Count; i++)
                            {
                                if (mnn.Hp < enemyminionsattack[i - 1].Angr || mnn.Hp < tempEnemyweaponattack)
                                {
                                    found = true;
                                    if (enemyminionsattack[i - 1].Angr <= tempEnemyweaponattack)
                                    {
                                        attack -= enemyminionsattack[enemyminionsattack.Count - 1].Angr;
                                        enemyminionsattack.RemoveAt(enemyminionsattack.Count - 1);
                                    }
                                    else
                                    {
                                        attack -= tempEnemyweaponattack;
                                        if (enemyweaponCanmultipleattack == 1) tempEnemyweaponattack = 0;
                                        enemyweaponCanmultipleattack--;
                                        //hastauntminion.Remove(mnn);
                                    }
                                    break;
                                }
                            }
                        }

                    }
                    if (p.enemyMinions.Count == 0 && p.enemyWeaponAttack >= 1) attack -= p.enemyWeaponAttack;
                }
                //Helpfunctions.Instance.ErrorLog(" attack " + attack);





                switch (p.enemyHeroAblility.card.name)
                {
                    case CardDB.cardName.steadyshot: directattack += 2; directenemyheropowerattack += 2; break; //hunter
                    case CardDB.cardName.ballistashot: directattack += 3; directenemyheropowerattack += 3; break; //hunter 3 damage
                    case CardDB.cardName.daggermastery: if (p.enemyWeaponAttack == 0) { attack += 1; potentialenemyattack += 1; } break; //rogue
                    case CardDB.cardName.poisoneddaggers: if (p.enemyWeaponAttack == 0) { attack += 2; potentialenemyattack += 2; } break; //rogue 2 att
                    case CardDB.cardName.shapeshift: { attack += 1; potentialenemyattack += 1; } break; //druid
                    case CardDB.cardName.direshapeshift: { attack += 2; potentialenemyattack += 2; } break; //druid 2att
                    case CardDB.cardName.fireblast: directattack += 1; directenemyheropowerattack += 1; break; //mage
                    case CardDB.cardName.fireblastrank2: directattack += 2; directenemyheropowerattack += 2; break; //mage 2att
                    case CardDB.cardName.mindspike: directattack += 2; directenemyheropowerattack += 2; break; //dark priest
                    case CardDB.cardName.mindshatter: directattack += 3; directenemyheropowerattack += 3; break; //dark priest rank2
                    case CardDB.cardName.lightningjolt: directattack += 2; break; //2dmg shamman weapon
                    case CardDB.cardName.lesserheal:
                        {
                            if (p.enemyMinions.Find(a => a.name == CardDB.cardName.auchenaisoulpriest && !a.silenced) != null)
                            {
                                directattack += 2;
                            }
                        }
                        break;
                    case CardDB.cardName.heal:
                        {
                            if (p.enemyMinions.Find(a => a.name == CardDB.cardName.auchenaisoulpriest && !a.silenced) != null)
                            {
                                directattack += 4;
                            }
                        }
                        break;
                    default: break;
                }
                if (hasInspire2Attack) { attack += 2; potentialenemyattack += 2; }

                concedevalue -= attack + directattack + p.destroyunitattack;
                //if (p.destroyunitattack >= 1) Helpfunctions.Instance.ErrorLog("destroyunitattack = " + p.destroyunitattack);
                potentialenemyattack += directattack;

            }





            if (availa.Contains(CardDB.cardIDEnum.CS2_051) && concedevalue <= 0 && (p.ownHero.numAttacksThisTurn == 0 || p.ownMinions.Find(a => a.allreadyAttacked) == null)) // taunt when enemy has lethal
            {
                p.callKid(kid4taunt, posi, ownplay);
                return;
            }

            if (availa.Contains(CardDB.cardIDEnum.CS2_050)) //always 1/1totem first
            {
                p.callKid(kid, posi, ownplay);
                return;
            }

            if (availa.Contains( CardDB.cardIDEnum.CS2_052)) //spellpower 1/3chance is enough 
            {
                p.callKid(kid2, posi, ownplay);
                return;
            }

            if (availa.Contains(CardDB.cardIDEnum.NEW1_009)) // heal 1/2
            {
                p.callKid(kid3heal, posi, ownplay);
                return;
            }

            if (availa.Contains( CardDB.cardIDEnum.CS2_051))
            {
                p.callKid(kid4taunt, posi, ownplay);
                
                return;
            }

            
        }
    }

}