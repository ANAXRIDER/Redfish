namespace HREngine.Bots
{
    using System;
    using System.Collections.Generic;
    public class BehaviorAggroshaman : Behavior
    {
        PenalityManager penman = PenalityManager.Instance;

        public override float getPlayfieldValue(Playfield p)
        {
            if (p.value >= -2000000) return p.value;
            float retval = 0;
            retval -= p.evaluatePenality;
            retval += p.owncards.Count * 5;

            
            //card counts
            retval += - (p.enemyDeckSize * 3);
            //Helpfunctions.Instance.ErrorLog("p.enemyDeckSize = " + p.enemyDeckSize);
            //Helpfunctions.Instance.ErrorLog("Hrtprozis.Instance.enemyDeckSize = " + Hrtprozis.Instance.enemyDeckSize);

            //check aggrodeck
            //cards check
            int Abusive_Sergeant = 0;
            int Leper_Gnome = 0;
            int Flame_Imp = 0;
            int Dire_Wolf_Alpha = 0;
            int Worgen_Infiltrator = 0;
            int Glaivezooka = 0;
            int Argent_Horseride = 0;
            int Arcane_Golem = 0;
            int Nerubian_Egg = 0;
            int Doomguard = 0;
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.CS2_188) Abusive_Sergeant = e.Value; //Abusive Sergeant CS2_188
                if (e.Key == CardDB.cardIDEnum.EX1_029) Leper_Gnome = e.Value; //Leper Gnome EX1_029
                if (e.Key == CardDB.cardIDEnum.EX1_319) Flame_Imp = e.Value; //Flame Imp EX1_319
                if (e.Key == CardDB.cardIDEnum.EX1_162) Dire_Wolf_Alpha = e.Value; //Dire Wolf Alpha EX1_162
                if (e.Key == CardDB.cardIDEnum.EX1_010) Worgen_Infiltrator = e.Value; //Worgen Infiltrator EX1_010
                if (e.Key == CardDB.cardIDEnum.GVG_043) Glaivezooka = e.Value; //Glaivezooka GVG_043
                if (e.Key == CardDB.cardIDEnum.AT_087) Argent_Horseride = e.Value; //Argent Horserider AT_087
                if (e.Key == CardDB.cardIDEnum.EX1_089) Arcane_Golem = e.Value; //Arcane Golem EX1_089
                //if (e.Key == CardDB.cardIDEnum.FP1_007) Nerubian_Egg = e.Value; //Nerubian Egg FP1_007
                if (e.Key == CardDB.cardIDEnum.EX1_310) Doomguard = e.Value; //Doomguard EX1_310
            }

            bool Aggrodeck = false;
            if (Abusive_Sergeant >= 1 || Leper_Gnome >= 1 || Flame_Imp >= 1 || Dire_Wolf_Alpha >= 1 || Worgen_Infiltrator >= 1 || Glaivezooka >= 1 || Argent_Horseride >= 1 || Arcane_Golem >= 1 || Nerubian_Egg >= 1)
            {
                Aggrodeck = true;
                //Helpfunctions.Instance.ErrorLog("aggrodeck : " + Aggrodeck);
            }


            //hp
            int phase = 0;
            if (p.ownHero.Hp + p.ownHero.armor >= 15) phase = 1; // over 15+
            if (p.ownHero.Hp + p.ownHero.armor <= 14 && p.ownHero.Hp + p.ownHero.armor >= 11) phase = 2; //11-14
            if (p.ownHero.Hp + p.ownHero.armor <= 10) phase = 3; // under 10
            if (p.ownHero.Hp + p.ownHero.armor >= 15 && (p.enemyHeroName == HeroEnum.hunter || p.enemyHeroName == HeroEnum.shaman)) phase = 4;
            if (p.ownHero.Hp + p.ownHero.armor <= 14 && p.enemyHeroName == HeroEnum.druid) phase = 5;
            if (Aggrodeck && p.ownHero.Hp + p.ownHero.armor <= 18) phase = 6;
            if (Aggrodeck && p.ownHero.Hp + p.ownHero.armor <= 19) phase = 7;

            switch (phase)
            {
                case 1: retval += 8 + 1 / 2 * ((p.ownHero.Hp + p.ownHero.armor) - 14); break; //15+ : 15 = 8 + 1, 16 = 8 + 2...
                case 2: retval += 2 * ((p.ownHero.Hp + p.ownHero.armor) - 10); break; //11-14 : 11=2 , 12 = 4... 14 = 8
                case 3: retval -= (10 - p.ownHero.Hp + p.ownHero.armor) * (10 - p.ownHero.Hp + p.ownHero.armor); break;
                case 4: retval += 8 + 2 * ((p.ownHero.Hp + p.ownHero.armor) - 14); break; //15+ : 15 = 8 + 2, 16 = 8 + 4...
                case 5: retval -= (14 - p.ownHero.Hp + p.ownHero.armor) + 80; break;
                case 6: retval += 4 * (p.ownHero.Hp + p.ownHero.armor); break;
                case 7: retval += 2 * (p.ownHero.Hp + p.ownHero.armor); break;
                default: break;
            }
            //hp enemy
            if (p.enemyHero.Hp + p.enemyHero.armor >= 11) 
            {
                retval += -4 * (p.enemyHero.Hp + p.enemyHero.armor); //y = -x +30
            }


            //if (p.owncards.Count <= 2) retval += -3* (p.enemyHero.Hp + p.enemyHero.armor);

            //iceblock
            if (p.enemyHero.immune && p.enemyHeroName == HeroEnum.mage)
            {
                retval += 10 * (20 - p.enemyHero.Hp + p.enemyHero.armor);//1:+27 2:+24... 10 = 0;
            }



            retval += p.ownMaxMana * 15 - p.enemyMaxMana * 15;

            retval -= p.manaTurnEnd * 2;


            //weapon
            /*
            bool hasweapon = false;
            foreach (Handmanager.Handcard hcc in p.owncards)
            {
                if (hcc.card.type == CardDB.cardtype.WEAPON || hcc.card.name == CardDB.cardName.musterforbattle) hasweapon = true;
                break;
            }
            foreach (Minion mnn in p.ownMinions)
            {
                if (mnn.name == CardDB.cardName.tirionfordring && !mnn.silenced) hasweapon = true;
                break;
            }
            
            if (p.ownHero.frozen)
            {
                retval -= p.ownWeaponAttack;
            }
            else if (hasweapon)
            {
                retval -= p.ownWeaponAttack * p.ownWeaponDurability; //attack value ++
            }
            else if (p.ownWeaponAttack >= 2)
            {
                retval += p.ownWeaponAttack;
            }
            */

            if (!p.ownHero.frozen)
            {
                retval += p.ownWeaponAttack + p.enemyWeaponDurability * 0.1f;
            }

            //enemy weapon
            if (!p.enemyHero.frozen)
            {
                retval -= p.enemyWeaponDurability * p.enemyWeaponAttack;
            }
            else
            {
                if (p.enemyHeroName != HeroEnum.mage && p.enemyHeroName != HeroEnum.priest && p.enemyHeroName != HeroEnum.warlock)
                {
                    retval += 11;
                }
            }


            //RR card draw value depending on the turn and distance to lethal
            //RR if lethal is close, carddraw value is increased


            if (Ai.Instance.lethalMissing <= 5 && p.turnCounter == 0) //RR
            {
                retval += p.owncarddraw * 100;
            }
            if (p.ownMaxMana <= 4)
            {
                retval += p.owncarddraw * 2;
            }
            else
            {
                //retval += p.owncarddraw * 5;
                // value card draw this turn > card draw next turn (the sooner the better)
                retval += (p.turnCounter < 2 ? p.owncarddraw * 5 : p.owncarddraw * 3);
            }

            retval -= (p.enemycarddraw - p.anzEnemyCursed) * 5;


            //int owntaunt = 0;
            int readycount = 0;
            int ownMinionsCount = 0;

            bool enemyhaspatron = false;

            //
            bool canPingMinions = (p.ownHeroAblility.card.name == CardDB.cardName.fireblast);
            bool hasPingedMinion = false;


            foreach (Minion m in p.enemyMinions)
            {
                if (m.name == CardDB.cardName.grimpatron && !m.silenced) enemyhaspatron = true;

                float currMinionValue = this.getEnemyMinionValue(m, p);

                // Give a bonus for 1 hp minions as a mage, since we can remove it easier in the future with ping.
                // But we make sure we only give this bonus once among all enemies. We also give another +1 bonus once if the atk >= 4.
                if (canPingMinions && !hasPingedMinion && currMinionValue > 2 && m.Hp == 1)
                {
                    currMinionValue -= 1;
                    canPingMinions = false;  // only 1 per turn (-1 bonus regardless of atk)
                    hasPingedMinion = true;
                }
                if (hasPingedMinion && currMinionValue > 2 && m.Hp == 1 && m.Angr >= 4)
                {
                    currMinionValue -= 1;
                    hasPingedMinion = false;  // only 1 per turn (-1 bonus additional for atk >= 4)
                }

                retval -= currMinionValue;

                //hasTank = hasTank || m.taunt;
            }

            bool hassecretkeeper = false;
            foreach (Minion m in p.ownMinions)
            {
                if (m.name == CardDB.cardName.secretkeeper && !m.silenced) hassecretkeeper = true;
            }

            bool useAbili = false;
            int usecoin = 0;
            foreach (Action a in p.playactions)
            {
                if (a.actionType == actionEnum.playcard && a.card.card.Secret && hassecretkeeper) retval += 4;
                //if (a.actionType == actionEnum.attackWithHero && p.enemyHero.Hp <= p.attackFaceHP) retval++;
                if (a.actionType == actionEnum.useHeroPower) useAbili = true;
                if (p.ownHeroName == HeroEnum.warrior && a.actionType == actionEnum.attackWithHero && useAbili) retval -= 1;
                //if (a.actionType == actionEnum.useHeroPower && a.card.card.name == CardDB.cardName.lesserheal && (!a.target.own)) retval -= 5;
                if (a.actionType != actionEnum.playcard) continue;
                if (a.card.card.name == CardDB.cardName.thecoin)
                {
                    usecoin = 1;
                }
                if (a.card.card.name == CardDB.cardName.innervate)
                {
                    usecoin = 2;
                }

                //save spell for all classes: (except for rouge if he has no combo)
                if (a.target == null) continue;
                if (p.ownHeroName != HeroEnum.thief && a.card.card.type == CardDB.cardtype.SPELL && (!a.target.own && a.target.isHero) && a.card.card.name != CardDB.cardName.shieldblock) retval -= 11;
                if (p.ownHeroName == HeroEnum.thief && a.card.card.type == CardDB.cardtype.SPELL && (a.target.isHero && !a.target.own)) retval -= 11;
            }
            if (usecoin >= 1 && useAbili && p.ownMaxMana <= 2) retval -= 40;
            if (usecoin >= 1 && p.manaTurnEnd >= usecoin && p.owncards.Count <= 8) retval -= 20 * p.manaTurnEnd;
            int heropowermana = p.ownHeroAblility.card.getManaCost(p, 2);

            if (p.manaTurnEnd >= heropowermana && !useAbili && p.ownAbilityReady)
            {
                if (p.ownHeroName == HeroEnum.pala) retval -= 10;
                else if (!(p.ownHeroName == HeroEnum.thief && (p.ownWeaponDurability >= 2 || p.ownWeaponAttack >= 2))) retval -= 15;
            }
            //if (usecoin && p.mana >= 1) retval -= 20;








            //aoe-spell check 
            //druid swipe
            int Swipe = 0; //Swipe CS2_012
            int Holy_Nova = 0; //Holy Nova CS1_112
            int Lightbomb = 0; //Lightbomb GVG_008
            int Lightning_Storm = 0; //Lightning Storm EX1_259
            int Flamestrike = 0; //Flamestrike CS2_032
            int Consecration = 0; //Consecration CS2_093
            int Hellfire = 0; //Hellfire CS2_062
            int Blade_Flurry = 0; //Blade Flurry CS2_233
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.CS2_012) Swipe = e.Value;//Swipe CS2_012
                if (e.Key == CardDB.cardIDEnum.CS1_112) Holy_Nova = e.Value;//Holy Nova CS1_112
                if (e.Key == CardDB.cardIDEnum.GVG_008) Lightbomb = e.Value;//Lightbomb GVG_008
                if (e.Key == CardDB.cardIDEnum.EX1_259) Lightning_Storm = e.Value;//Lightning Storm EX1_259
                if (e.Key == CardDB.cardIDEnum.CS2_032) Flamestrike = e.Value;//Flamestrike CS2_032
                if (e.Key == CardDB.cardIDEnum.CS2_093) Consecration = e.Value;//Consecration CS2_093
                if (e.Key == CardDB.cardIDEnum.CS2_062) Hellfire = e.Value;//Hellfire CS2_062
                if (e.Key == CardDB.cardIDEnum.CS2_233) Blade_Flurry = e.Value;//Blade Flurry CS2_233
            }

            //killcard check
            int Big_Game_Hunter = 0;
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.EX1_005) Big_Game_Hunter = e.Value; //Big Game Hunter EX1_005
            }

            bool doomsayeron = false;
            foreach (Minion m in p.enemyMinions)
            {
                if (m.name == CardDB.cardName.doomsayer && !m.silenced) doomsayeron = true;
            }

            if (!doomsayeron)
            {
                foreach (Minion m in p.ownMinions)
                {
                    ////aoe-spell (3+ my minion)
                    //if (p.ownMinions.Count >= 3 && m.name != CardDB.cardName.twilightdrake && p.ownloatheb != 0)
                    //{
                    //    //swipe hp1 minion
                    //    if (p.enemyHeroName == HeroEnum.druid && p.enemyMaxMana >= 6 && Swipe == 0 && m.Hp == 1) retval -= m.Hp + m.Angr * 2;
                    //    //holynova
                    //    if (p.enemyHeroName == HeroEnum.priest && p.enemyMaxMana >= 7 && Holy_Nova == 0 && m.Hp <= 2) retval -= m.Hp + m.Angr * 2;
                    //    //Lightbomb
                    //    if (p.enemyHeroName == HeroEnum.priest && p.enemyMaxMana >= 8 && Lightbomb == 0 && m.Hp <= m.Angr) retval -= m.Hp + m.Angr * 2;
                    //    //Lightning Storm EX1_259
                    //    if (p.enemyHeroName == HeroEnum.shaman && p.enemyMaxMana >= 5 && Lightning_Storm == 0 && m.Hp <= 3) retval -= m.Hp + m.Angr * 2;
                    //    //Flamestrike CS2_032
                    //    if (p.enemyHeroName == HeroEnum.mage && p.enemyMaxMana >= 8 && Flamestrike == 0 && m.Hp <= 4) retval -= m.Hp + m.Angr * 2;
                    //    //Consecration CS2_093
                    //    if (p.enemyHeroName == HeroEnum.pala && p.enemyMaxMana >= 6 && Consecration == 0 && m.Hp <= 2) retval -= m.Hp + m.Angr * 2;
                    //    //Hellfire CS2_062
                    //    if (p.enemyHeroName == HeroEnum.warlock && p.enemyMaxMana >= 6 && Hellfire == 0 && m.Hp <= 3) retval -= m.Hp + m.Angr * 2;
                    //    //Blade Flurry CS2_233
                    //    if (p.enemyHeroName == HeroEnum.thief && p.enemyMaxMana >= 5 && Blade_Flurry == 0 && m.Hp <= p.enemyWeaponAttack) retval -= m.Hp + m.Angr * 2;
                    //}

                    //
                    if (p.ownHeroName == HeroEnum.warlock && (TAG_RACE)m.handcard.card.race == TAG_RACE.DEMON) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.hunter && (TAG_RACE)m.handcard.card.race == TAG_RACE.PET) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.shaman && (TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.pala && m.name == CardDB.cardName.silverhandrecruit) retval += 0.1f; ;
                    if (p.ownHeroName == HeroEnum.mage && (TAG_RACE)m.handcard.card.race == TAG_RACE.MECHANICAL) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.mage && m.name == CardDB.cardName.flamewaker) retval += 5;

                    //anti aoe
                    if (m.handcard.card.anti_aoe_minion >= 1 && !m.silenced && p.ownMinions.Count - p.enemyMinions.Count >= 2) retval += m.handcard.card.anti_aoe_minion;
                    if (m.divineshild && p.ownMinions.Count - p.enemyMinions.Count >= 2) retval += m.Angr * 2;

                    //kill card
                    //if (p.enemyMaxMana >= 5 && Big_Game_Hunter == 0 && m.Angr >= 7) retval -= (m.Hp + m.Angr) * 2 / 3; //if enemy not uses bgh, minion value is half.

                    retval += 2;
                    retval += m.Hp * 1;
                    retval += m.Angr * 2;
                    //retval += m.handcard.card.rarity;
                    if (m.handcard.card.isSpecialMinion) retval += 1;

                    if (m.windfury) retval += m.Angr * 2;
                    if (m.divineshild) retval += m.Angr * 3/2;
                    if (m.stealth) retval += 1;
                    if (m.taunt) retval += 2;
                    if (m.handcard.card.isSpecialMinion)
                    {
                        retval += 1;
                        if (!m.taunt && m.stealth) retval += (m.Angr < 4 ? 2 : 5);
                    }
                    if (m.handcard.card.name == CardDB.cardName.silverhandrecruit && m.Angr == 1 && m.Hp == 1) retval -= 2;
                    if (m.handcard.card.name == CardDB.cardName.direwolfalpha || m.handcard.card.name == CardDB.cardName.flametonguetotem || m.handcard.card.name == CardDB.cardName.stormwindchampion || m.handcard.card.name == CardDB.cardName.raidleader) retval += 10;
                    if (m.destroyOnEnemyTurnEnd || m.destroyOnEnemyTurnStart) retval -= m.Hp * 1 + m.Angr * 2 + m.handcard.card.rarity;
                }
            }

            

            foreach (Minion m in p.enemyMinions)
            {
                retval -= this.getEnemyMinionValue(m, p);
            }


            retval -= 5 * p.enemySecretCount;
            retval += 3 * p.ownSecretsIDList.Count;


            //retval -= 2 * p.lostDamage;//damage which was to high (like killing a 2/1 with an 3/3 -> => lostdamage =2
            //retval -= p.lostWeaponDamage;
            if (p.ownMinions.Count == 0) retval -= 5;

            //if (Settings.Instance.savecardswhencontrol)
            //{
            //    int potentialattack = 0;
            //    foreach (Minion mnn in p.ownMinions)
            //    {
            //        if (mnn.canAttackNormal) potentialattack += mnn.Angr;
            //    }
            //    if (p.enemyMinions.Count == 0 && p.ownMinions.Count >= 4 && potentialattack >= 12)
            //    {
            //        retval += 50;
            //        retval += 20 * p.owncards.Count;
            //    }
            //}

            

            if (p.enemyHero.Hp <= 0)
            {
                if (p.turnCounter <= 1)
                {
                    retval += 10000;
                }
                else
                {
                    if (p.turnCounter >= 2)
                    {
                        retval += 50;
                    }
                    foreach (Minion mnn in p.ownMinions)
                    {
                        if (mnn.name == CardDB.cardName.ragnarosthefirelord && mnn.silenced && (p.enemyHero.Hp + p.enemyHero.armor <= 8)) retval += 5000;
                    }
                    retval += 5;//10000
                    if (p.numPlayerMinionsAtTurnStart == 0) retval += 3; // if we can kill the enemy even after a board clear, bigger bonus
                    if (p.loathebLastTurn > 0) retval += 15;  // give a bonus to turn 2 sims where we played loatheb in turn 1 to protect our lethal board

                }
            }
            else if (p.ownHero.Hp > 0)
            {
                // if our damage on board is lethal, give a strong bonus so enemy AI avoids this outcome in its turn (i.e. AI will clear our minions if it can instead of ignoring them)
                if (p.turnCounter == 1 && p.guessHeroDamage(true) >= p.enemyHero.Hp + p.enemyHero.armor) retval += 5;
            }



            //soulfire etc
            int deletecardsAtLast = 0;
            foreach (Action a in p.playactions)
            {
                if (a.actionType != actionEnum.playcard) continue;
                if (a.card.card.name == CardDB.cardName.soulfire || a.card.card.name == CardDB.cardName.doomguard || a.card.card.name == CardDB.cardName.succubus) deletecardsAtLast = 1;
                if (deletecardsAtLast == 1 && !(a.card.card.name == CardDB.cardName.soulfire || a.card.card.name == CardDB.cardName.doomguard || a.card.card.name == CardDB.cardName.succubus)) retval -= 20;
            }

            if (p.enemyHero.Hp >= 1 && p.ownHero.Hp <= 0)
            {
                //Helpfunctions.Instance.ErrorLog("turncounter " + p.turnCounter + " " + retval);

                if (p.turnCounter == 0) // own turn 
                {
                    //worst case: we die on own turn
                    retval += p.owncarddraw * 100;
                    retval = -10000;
                }
                else
                {
                    if (p.turnCounter == 1) // enemys first turn
                    {
                        //retval += p.owncarddraw * 500;
                        retval -= 1000;
                    }
                    if (p.turnCounter >= 2)
                    {
                        //carddraw next turn doesnt count this turn :D
                        retval -= 100;
                        if (p.loathebLastTurn > 0) retval += 50;
                    }
                }
            }

            //if (p.ownHero.Hp <= 0 && p.turnCounter < 2) retval = -10000;

            // give a bonus for making the enemy spend more mana dealing with our board, so boards where the enemy makes different plays
            // aren't considered as equal value (i.e. attacking the enemy and making him spend mana to heal vs not attacking at all)
            if (p.turnCounter == 1 || p.turnCounter == 3) retval += p.enemyMaxMana - p.mana;

            p.value = retval;
            return retval;
        }

        //other value of the board for enemys turn? (currently the same as getplayfield value)
        public override float getPlayfieldValueEnemy(Playfield p)
        {
            return getPlayfieldValue(p);
        }


        public override float getEnemyMinionValue(Minion m, Playfield p)
        {
            float retval = 2;
            retval += m.Hp * 1;
            if (m.name == CardDB.cardName.doomsayer && m.Angr == 0 && m.silenced) retval -= m.Hp * 2;
            if (!m.frozen && !((m.name == CardDB.cardName.ancientwatcher || m.name == CardDB.cardName.ragnarosthefirelord) && !m.silenced))
            {
                retval += m.Angr * 2;
                if (m.windfury) retval += m.Angr * 2;
                //if (m.Angr >= 5) retval += m.Angr - 2;
                //if (m.Angr >= 7) retval += m.Angr - 2;
            }

            
            if (m.taunt) retval++;
            if (m.divineshild) retval += m.Angr;
            //if (m.divineshild && m.Angr == 1 && p.enemyHeroName == HeroEnum.pala) retval += 5;
            if (m.frozen) retval -= 1; // because its bad for enemy :D
            if (m.poisonous) retval += 4;
            //retval += m.handcard.card.rarity;
            //
            if (p.enemyHeroName == HeroEnum.hunter && p.ownHero.Hp <= 12) retval += m.Angr;
            if (p.enemyHeroName == HeroEnum.warlock && (TAG_RACE)m.handcard.card.race == TAG_RACE.DEMON) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.hunter && (TAG_RACE)m.handcard.card.race == TAG_RACE.PET) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.shaman && (TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.pala && m.name == CardDB.cardName.silverhandrecruit) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.mage && (TAG_RACE)m.handcard.card.race == TAG_RACE.MECHANICAL) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.mage && m.name == CardDB.cardName.flamewaker) retval += 5;
            if (m.handcard.card.deathrattle && !m.silenced) retval += Math.Max(m.handcard.card.cost - 2, 1); //not priority to deathrattle (bad for us)
            //
            if (m.handcard.card.targetPriority >= 1 && !m.silenced)
            {
                retval += m.handcard.card.targetPriority;
            }
            //if (m.Angr >= 4) retval += m.Angr;
            //if (m.Angr >= 7) retval += m.Angr;
            if (m.name == CardDB.cardName.nerubianegg && m.Angr <= 3 && !m.taunt) retval = 0;
            return retval;
        }


    }

}