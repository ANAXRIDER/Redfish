using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{

    class Sim_NEW1_019 : SimTemplate //knifejuggler
    {

        //    fügt einem zufälligen feind 1 schaden zu, nachdem ihr einen diener herbeigerufen habt.
        public override void onMinionWasSummoned(Playfield p, Minion triggerEffectMinion, Minion summonedMinion)
        {
            if (triggerEffectMinion.entityID != summonedMinion.entityID && triggerEffectMinion.own == summonedMinion.own)
            {
                List<Minion> temp = (triggerEffectMinion.own) ? p.enemyMinions : p.ownMinions;

                if (temp.Count >= 1)
                {
                    //search Minion with lowest hp
                    Minion enemy = temp[0];
                    int minhp = 10000;
                    bool found = false;
                    bool hastaunt = false;
                    bool hp1taunt = false;
                    int hp1count = 0;
                    int hp2pluscount = 0;
                    int acolyteposi = 0;
                    bool hasenemyacolyte = false;
                    foreach (Minion m in temp)
                    {
                        //if (m.name == CardDB.cardName.nerubianegg && m.Hp >= 2) continue; //dont attack nerubianegg!
                        //if (m.handcard.card.isToken && m.Hp == 1) continue;
                        //if (m.name == CardDB.cardName.defender) continue;
                        //if (m.name == CardDB.cardName.spellbender) continue;
                        if (m.name == CardDB.cardName.acolyteofpain && !m.silenced && m.Hp >= 2)
                        {
                            hasenemyacolyte = true;
                            acolyteposi = m.zonepos;
                            break;
                        }
                        if (m.Hp == 1 && m.taunt)
                        {
                            enemy = m;
                            hp1taunt = true;
                            break;
                        }
                        if (m.Hp >= 2 && minhp > m.Hp)
                        {
                            enemy = m;
                            minhp = m.Hp;
                            found = true;
                            
                            if (m.taunt) { hastaunt = true; break; }
                        }
                        if (m.Hp >= 2) hp2pluscount++;
                        if (m.Hp == 1 || m.divineshild)
                        {
                            enemy = m;
                            hp1count++;
                        }
                    }

                    //Helpfunctions.Instance.ErrorLog("hp2pluscount " + hp2pluscount);
                    //Helpfunctions.Instance.ErrorLog("hp1count " + hp1count);
                    if (hasenemyacolyte)
                    {
                        p.minionGetDamageOrHeal(temp[acolyteposi-1], 1);
                    }
                    else if (hp1taunt)
                    {
                        p.minionGetDamageOrHeal(enemy, 1);
                    }
                    else if (hastaunt)
                    {
                        p.minionGetDamageOrHeal(enemy, 1);
                        //if (enemy.Hp == 1) p.minionGetDamageOrHeal(enemy, 1);
                    }
                    else if (enemy.Hp == 1 && hp2pluscount == 0) 
                    {
                        p.minionGetDamageOrHeal(enemy, 1);
                    }
                    else if (hp1count >= 1 && hp2pluscount >= 1)
                    {
                        //Helpfunctions.Instance.ErrorLog("hp2pluscount " + hp2pluscount);
                        p.minionGetDamageOrHeal(triggerEffectMinion.own ? p.enemyHero : p.ownHero, 1);
                    }
                    else if (enemy.divineshild)
                    {
                        p.minionGetDamageOrHeal(enemy, 1);
                    }
                    else if (found && temp.Count == 1)
                    {
                        p.minionGetDamageOrHeal(temp[0], 1);
                    }
                    else if (found && enemy.Hp == 1)
                    {
                        p.minionGetDamageOrHeal(temp[0], 1);
                    }
                    else if (found)
                    {
                        p.minionGetDamageOrHeal(temp[0], 1);
                    }
                    else
                    {
                        p.minionGetDamageOrHeal(triggerEffectMinion.own ? p.enemyHero : p.ownHero, 1);
                    }

                }
                else
                {
                    p.minionGetDamageOrHeal(triggerEffectMinion.own ? p.enemyHero : p.ownHero, 1);
                }

                triggerEffectMinion.stealth = false;
            }
        }
    }
}