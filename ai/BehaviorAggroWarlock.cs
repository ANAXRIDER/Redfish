namespace HREngine.Bots
{
    using System;
    using System.Collections.Generic;
    public class BehaviorAggroWarlock : Behavior
    {
        PenalityManager penman = PenalityManager.Instance;

        //public int destroyunitattack;

        public static readonly BehaviorAggroWarlock instance = new BehaviorAggroWarlock();

        static BehaviorAggroWarlock() { } // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit

        public static BehaviorAggroWarlock Instance
        {
            get
            {
                return instance;
            }
        }

        public int lethalMissed;

        public override float getPlayfieldValue(Playfield p)
        {
            if (p.value >= -2000000) return p.value;
            float retval = 0;
            retval -= p.evaluatePenality;
            //retval += p.owncards.Count * 2;

            //retval -= p.enemyspellpower * 5;

            if (p.enemyMinions.Count == 0) retval += p.spellpower * 3 ;

            //card counts
            retval += - (p.enemyDeckSize * 3);
            //Helpfunctions.Instance.ErrorLog("p.enemyDeckSize = " + p.enemyDeckSize);
            //Helpfunctions.Instance.ErrorLog("Hrtprozis.Instance.enemyDeckSize = " + Hrtprozis.Instance.enemyDeckSize);




            retval -= p.owedRecall * 3;



            
            











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
            //else { Helpfunctions.Instance.ErrorLog("aggrodeck : " + Aggrodeck); }

            bool hastaunt = false;

            foreach (Minion mnn in p.enemyMinions)
            {
                if (mnn.taunt) hastaunt = true;
            }








            //hp
            int phase = 0;
            if (p.ownHero.Hp + p.ownHero.armor >= 15) phase = 1; // over 15+
            if (p.ownHero.Hp + p.ownHero.armor <= 14 && p.ownHero.Hp + p.ownHero.armor >= 11) phase = 2; //11-14
            if (p.ownHero.Hp + p.ownHero.armor <= 10) phase = 3; // under 10
            //if (p.ownHero.Hp + p.ownHero.armor >= 15 && (p.enemyHeroName == HeroEnum.hunter || p.enemyHeroName == HeroEnum.shaman)) phase = 4;
            //if (p.ownHero.Hp + p.ownHero.armor <= 14 && p.enemyHeroName == HeroEnum.druid && p.ownMaxMana >= 8 && p.anzOwnLoatheb == 0 && !hastaunt) phase = 5;
            //if (Aggrodeck && p.ownHero.Hp + p.ownHero.armor <= 10) phase = 6;
            //if (Aggrodeck && p.ownHero.Hp + p.ownHero.armor >= 11) phase = 7;

            //Helpfunctions.Instance.ErrorLog("phase : " + phase);
            
            float hpvalue = 0;
            switch (phase)
            {
                case 1: hpvalue += 8 + ((p.ownHero.Hp + p.ownHero.armor) - 14) * 0.5f; break; 
                    //15hp이상 15:8.5 16=9 0.5씩오름
                case 2: hpvalue += 2 * ((p.ownHero.Hp + p.ownHero.armor) - 10); break; 
                    //11~14 11=2 12 =4 13 =6 14=8
                case 3: hpvalue -= (10 - (p.ownHero.Hp + p.ownHero.armor)) * (10 - (p.ownHero.Hp + p.ownHero.armor)); break;
                    //~10 10=0 9면 -1 제곱으로 8이면 -4..
                case 4: hpvalue += 8 + 2 * ((p.ownHero.Hp + p.ownHero.armor) - 14); break; 
                    //15피이상 적냥꾼 주술 15: 10 16: 18
                case 5: hpvalue -= (14 - p.ownHero.Hp + p.ownHero.armor) + 80; break;
                case 6: hpvalue += 4 * (p.ownHero.Hp + p.ownHero.armor); break;
                case 7: hpvalue += 2 * (p.ownHero.Hp + p.ownHero.armor); break;
                default: break;
            }
            retval += hpvalue;
            //Helpfunctions.Instance.ErrorLog("phase : " + phase);
            //Helpfunctions.Instance.ErrorLog("hpvalue : " + hpvalue);




            int Fireball = 0;
            int Lava_Burst = 0;

            int Kill_Command = 0;
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.CS2_029) Fireball = e.Value; //Fireball CS2_029
                if (e.Key == CardDB.cardIDEnum.EX1_241) Lava_Burst = e.Value; //Lava Burst EX1_241
                if (e.Key == CardDB.cardIDEnum.EX1_539) Kill_Command = e.Value; //Kill_Command EX1_539

            }

            //Helpfunctions.Instance.ErrorLog("Fireball count : " +Fireball);



            int enemypotentialattack = 0;
            switch (p.enemyHeroAblility.card.name)
            {
                case CardDB.cardName.steadyshot: enemypotentialattack += 2; break; //hunter
                case CardDB.cardName.ballistashot: enemypotentialattack += 3; break; //hunter 3 damage
                case CardDB.cardName.daggermastery: enemypotentialattack += 1; break; //rogue
                case CardDB.cardName.poisoneddaggers: enemypotentialattack += 2; break; //rogue 2 att
                case CardDB.cardName.shapeshift: enemypotentialattack += 1; break; //druid
                case CardDB.cardName.direshapeshift: enemypotentialattack += 2; break; //druid 2att
                case CardDB.cardName.fireblast: enemypotentialattack += 1; break; //mage
                case CardDB.cardName.fireblastrank2: enemypotentialattack += 2; break; //mage 2att
                case CardDB.cardName.mindspike: enemypotentialattack += 2; break; //dark priest
                case CardDB.cardName.mindshatter: enemypotentialattack += 3; break; //dark priest rank2
                case CardDB.cardName.lightningjolt: enemypotentialattack += 2; break; //2dmg shamman weapon
                default: break;
            }

            int enemypotentialattacktotal = enemypotentialattack;
            foreach (Minion mnn in p.enemyMinions)
            {
                enemypotentialattacktotal += mnn.Angr;
                if (mnn.windfury) enemypotentialattacktotal += mnn.Angr;
            }
            enemypotentialattacktotal += p.enemyWeaponAttack;

            //Helpfunctions.Instance.ErrorLog("enemypotentialattacktotal count : " + enemypotentialattacktotal);

            switch (p.enemyHeroName)
            {
                case HeroEnum.mage:
                    {
                        if (Fireball == 0 && p.ownHero.Hp + p.ownHero.armor <= 7) retval -= 50;
                        else if (Fireball == 0 && p.ownHero.Hp + p.ownHero.armor <= enemypotentialattacktotal + 6)
                        {
                            retval -= enemypotentialattacktotal;
                            //Helpfunctions.Instance.ErrorLog("enemypotentialattack * 50 : " + enemypotentialattacktotal * 50);
                        }
                        break;
                    }
                case HeroEnum.shaman:
                    {
                        if (Lava_Burst == 0 && p.ownHero.Hp + p.ownHero.armor <= 5) retval -= 50; 
                        else if (Lava_Burst == 0 && p.ownHero.Hp + p.ownHero.armor <= enemypotentialattacktotal + 3)
                        {
                            retval -= enemypotentialattacktotal;
                            //Helpfunctions.Instance.ErrorLog("enemypotentialattack * 50 : " + enemypotentialattacktotal * 50);
                        }
                        break;
                    }
                case HeroEnum.hunter:
                    {
                        if (Kill_Command == 0 && p.ownHero.Hp + p.ownHero.armor <= 7) retval -= 50;
                        else if (Kill_Command == 0 && p.ownHero.Hp + p.ownHero.armor <= enemypotentialattacktotal + 5)
                        {
                            retval -= enemypotentialattacktotal;
                            //Helpfunctions.Instance.ErrorLog("enemypotentialattack * 50 : " + enemypotentialattacktotal * 50);
                        }
                        break;
                    }

                default:
                    {
                        if (p.ownHero.Hp + p.ownHero.armor <= enemypotentialattacktotal + 3)
                        {
                            retval -= enemypotentialattacktotal;
                        }
                        break;
                    }
            }














            //hp enemy
            float enemyherohpvalue = 0;
            if (p.enemyHero.Hp + p.enemyHero.armor <= 5)
            {
                if (p.enemyHero.Hp + p.enemyHero.armor <= 0)
                {
                    
                }
                else enemyherohpvalue += -8 * (p.enemyHero.Hp + p.enemyHero.armor) + 90; //y = -10x + 100
            }
            else if (p.enemyHero.Hp + p.enemyHero.armor <= 10) 
            {
                enemyherohpvalue += -4 * (p.enemyHero.Hp + p.enemyHero.armor) + 70; //y = -4x + 70
                //retval += -3 * (p.enemyHero.Hp + p.enemyHero.armor) + 60;
            }
            else if (p.enemyHero.Hp + p.enemyHero.armor >= 11 )//&& p.enemyHero.Hp + p.enemyHero.armor <= 20)
            {
                enemyherohpvalue += -2 * (p.enemyHero.Hp + p.enemyHero.armor) + 50; //y = - 2x + 50
            }
            //else if (p.enemyHero.Hp + p.enemyHero.armor >= 21)
            //{
            //    retval += -(p.enemyHero.Hp + p.enemyHero.armor) + 30; //y= - x + 30
            //}

            //if (p.owncards.Count <= 2) retval += -3* (p.enemyHero.Hp + p.enemyHero.armor);

            //Helpfunctions.Instance.ErrorLog("enemyherohpvalue : " + enemyherohpvalue);
            retval += enemyherohpvalue;

            retval += p.ownMaxMana * 15 - p.enemyMaxMana * 15;

            if (p.ownMaxMana <= 6) retval -= p.manaTurnEnd * 3;
            else retval -= p.manaTurnEnd;


            if (!p.ownHero.frozen)
            {
                retval += p.ownWeaponAttack + p.enemyWeaponDurability * 0.1f;
                if (p.ownWeaponName == CardDB.cardName.spiritclaws && p.spellpower >= 1) retval -= 2;
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

            //Helpfunctions.Instance.ErrorLog("lethalMissing: " + Ai.Instance.lethalMissing);
            if (p.owncards.Count <= 8 && p.ownDeckSize >= 1)
            {
                if (Ai.Instance.lethalMissing <= 4 && p.turnCounter == 0 && !p.enemyHero.immune) //RR
                {
                    //Helpfunctions.Instance.ErrorLog("lethalMissing: " + Ai.Instance.lethalMissing);
                    retval += p.owncarddraw * 20;
                }
                if (p.ownMaxMana <= 4)
                {
                    retval += p.owncarddraw * 2;
                }
                else
                {
                    //retval += p.owncarddraw * 5;
                    // value card draw this turn > card draw next turn (the sooner the better)
                    retval += (p.turnCounter < 2 ? p.owncarddraw * 3 : p.owncarddraw * 2);
                }
            }


            //enemy draw rogue

            int Shadowstep = 0;
            int Gang_Up = 0;
            int Coldlight_Oracle = 0;

            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.EX1_144) Shadowstep = e.Value; //Shadowstep EX1_144
                if (e.Key == CardDB.cardIDEnum.BRM_007) Gang_Up = e.Value; //Gang_Up BRM_007
                if (e.Key == CardDB.cardIDEnum.EX1_050) Coldlight_Oracle = e.Value; //Coldlight_Oracle EX1_050
            }
            //Helpfunctions.Instance.ErrorLog("Shadowstep: " + Shadowstep);
            //Helpfunctions.Instance.ErrorLog("Gang_Up: " + Gang_Up);
            //Helpfunctions.Instance.ErrorLog("Coldlight_Oracle: " + Coldlight_Oracle);


            if (Coldlight_Oracle >= 1)
            {
                if (p.enemyHeroName == HeroEnum.thief) 
                {
                    if (Shadowstep >= 1 || Gang_Up >= 1)
                    {
                        retval -= p.owncarddraw * 10;
                        if (p.owncards.Count >= 6) retval -= p.owncarddraw * 5;
                    }
                    else
                    {
                        retval -= p.owncarddraw * 5;
                        if (p.owncards.Count >= 7) retval -= p.owncarddraw * 5;
                    }
                }
            }


            retval += (p.anzEnemyCursed) * 5;
            if (p.enemycarddraw >= 3) retval -= (p.enemycarddraw - 1) * 3;


            foreach (Handmanager.Handcard hcc in p.owncards)
            {
                if (hcc.card.name == CardDB.cardName.cursed && hcc.getManaCost(p) <= p.manaTurnEnd) retval -= 10;
            }




            if (p.CountEnemyAcolyteStarted == 1 && p.enemycarddraw >= 2 || p.CountEnemyAcolyteStarted == 2 && p.enemycarddraw >= 3) retval -= p.enemycarddraw * 7;


            bool hassecretkeeper = false;
            int jugglercount = 0;
            int deathrattlecount = 0;
            foreach (Minion m in p.ownMinions)
            {
                if (m.name == CardDB.cardName.secretkeeper && !m.silenced) hassecretkeeper = true;
                if (m.name == CardDB.cardName.knifejuggler) jugglercount++;
                if (m.name == CardDB.cardName.possessedvillager) deathrattlecount++;
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
                if ((p.ownHeroAblility.card.name == CardDB.cardName.thesilverhand ||
                   p.ownHeroAblility.card.name == CardDB.cardName.reinforce) && p.ownMinions.Count <= 6) retval -= 10;
                else if (p.ownHeroAblility.card.name == CardDB.cardName.lifetap && p.ownHero.Hp >= 12) retval -= 30;
                else if (!(p.ownHeroAblility.card.name == CardDB.cardName.daggermastery && (p.ownWeaponDurability >= 2 || p.ownWeaponAttack >= 2))
                    && !(p.ownHeroAblility.card.name == CardDB.cardName.thesilverhand ||
                   p.ownHeroAblility.card.name == CardDB.cardName.reinforce ||
                   p.ownHeroAblility.card.name == CardDB.cardName.totemiccall ||
                   p.ownHeroAblility.card.name == CardDB.cardName.totemicslam ||
                   p.ownHeroAblility.card.name == CardDB.cardName.inferno) && p.ownMinions.Count == 7
                    ) retval -= 20;
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
            int Unleash_the_Hounds = 0; //Unleash the Hounds EX1_538
            int Maelstrom_Portal = 0;// Maelstrom Portal KAR_073
            int Fan_of_Knives = 0;// Fan of Knives EX1_129
            int Execute = 0; //Execute CS2_108
            int Ravaging_Ghoul = 0; // Ravaging Ghoul OG_149
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
                if (e.Key == CardDB.cardIDEnum.EX1_538) Unleash_the_Hounds = e.Value;//Unleash the Hounds EX1_538
                if (e.Key == CardDB.cardIDEnum.KAR_073) Maelstrom_Portal = e.Value;//Maelstrom Portal KAR_073
                if (e.Key == CardDB.cardIDEnum.EX1_129) Fan_of_Knives = e.Value;//Fan of Knives EX1_129
                if (e.Key == CardDB.cardIDEnum.CS2_108) Execute = e.Value;//Execute CS2_108
                if (e.Key == CardDB.cardIDEnum.OG_149) Ravaging_Ghoul = e.Value;//Ravaging Ghoul OG_149
            }
            //Helpfunctions.Instance.ErrorLog("Execute" + Execute + " Ravaging_Ghoul" + Ravaging_Ghoul);


            //killcard check
            int Big_Game_Hunter = 0;
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.EX1_005) Big_Game_Hunter = e.Value; //Big Game Hunter EX1_005
            }

            bool enemyDoomsayer = false;
            bool ownDoomsayer = false;

            if (p.enemyMinions.Find(m => m.name == CardDB.cardName.doomsayer && !m.silenced) != null) enemyDoomsayer = true;
            if (p.ownMinions.Find(m => m.name == CardDB.cardName.doomsayer && !m.silenced) != null) ownDoomsayer = true;

            if (enemyDoomsayer)
            {
                retval -= 10 * p.ownMinions.Count ;
            }

            //if (p.diedMinions != null)
            //{
            //    Helpfunctions.Instance.ErrorLog("*********************************************************");
            //    foreach (GraveYardItem gy in p.diedMinions)
            //    {
            //        if (!gy.own && gy.cardid == CardDB.cardIDEnum.EX1_007) retval += 7 * gy.entity; //acoyte
            //    }
            //}
            



            bool hasowntauntminion = false;
            if (!enemyDoomsayer && !ownDoomsayer)
            {
                bool impgangbossgoodposition = true;
                bool hasenemytaunt = false;
                bool darkshirecouncilmanBadposition = false;
                bool enemyhaschillmaw = false;
                bool abomination = false;
                foreach (Minion mmm in p.enemyMinions)
                {
                    if (mmm.Angr >= 4) impgangbossgoodposition = false;
                    if (mmm.taunt && mmm.Angr >= 1) hasenemytaunt = true;
                    if (mmm.Hp >= 2 && mmm.Angr >= 3) darkshirecouncilmanBadposition = true;
                    if (!mmm.silenced && mmm.name == CardDB.cardName.chillmaw) enemyhaschillmaw = true;
                    if (!mmm.silenced && mmm.name == CardDB.cardName.abomination) abomination = true;
                }

                int strongesthp1minion = 0;
                foreach (Minion mmnn in p.ownMinions)
                {
                    if (mmnn.Hp == 1)
                    {
                        if (mmnn.Angr >= strongesthp1minion) strongesthp1minion = mmnn.Angr * 2 + mmnn.Hp;
                    }
                }

                //mage
                if (p.enemyHeroAblility.card.name == CardDB.cardName.fireblast && p.ownMaxMana >= 7) retval -= strongesthp1minion;

                foreach (Minion m in p.ownMinions)
                {
                    //aoe - spell(3 + my minion)
                    if (p.ownMinions.Count >= 3 && p.anzOwnLoatheb == 0 && p.enemyMinions.Count == 0)
                    {
                        //unleash
                        if (p.enemyHeroName == HeroEnum.hunter && p.enemyMaxMana >= 3 && Unleash_the_Hounds == 0 && (m.Hp >= 2 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //swipe hp1 minion
                        if (p.enemyHeroName == HeroEnum.druid && p.enemyMaxMana >= 4 && Swipe == 0 && (m.Hp >= 2 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //holynova
                        if (p.enemyHeroName == HeroEnum.priest && p.enemyMaxMana >= 4 && Holy_Nova == 0 && (m.Hp >= 3 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //Lightbomb
                        //if (p.enemyHeroName == HeroEnum.priest && p.enemyMaxMana >= 8 && Lightbomb == 0 && m.Hp <= m.Angr) retval -= m.Hp + m.Angr * 2;
                        //Lightning Storm EX1_259
                        if (p.enemyHeroName == HeroEnum.shaman && p.enemyMaxMana - p.enemyRecall >= 3 && Lightning_Storm == 0 && (m.Hp >= 3 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        else if (p.enemyHeroName == HeroEnum.shaman && p.enemyMaxMana - p.enemyRecall >= 2 && Maelstrom_Portal == 0 && (m.Hp >= 2 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //Flamestrike CS2_032
                        if (p.enemyHeroName == HeroEnum.mage && p.enemyMaxMana >= 6 && Flamestrike == 0 && (m.Hp >= 5 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //Consecration CS2_093
                        if (p.enemyHeroName == HeroEnum.pala && p.enemyMaxMana >= 3 && Consecration == 0 && (m.Hp >= 3 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;
                        //Hellfire CS2_062
                        //if (p.enemyHeroName == HeroEnum.warlock && p.enemyMaxMana >= 6 && Hellfire == 0 && m.Hp <= 3) retval -= m.Hp + m.Angr * 2;
                        //Blade Flurry CS2_233
                        //if (p.enemyHeroName == HeroEnum.thief && p.enemyMaxMana >= 5 && Blade_Flurry == 0 && m.Hp <= p.enemyWeaponAttack) retval -= m.Hp + m.Angr * 2;
                        if (p.enemyHeroName == HeroEnum.thief && p.enemyMaxMana >= 3 && Fan_of_Knives == 0 && (m.Hp >= 2 || m.divineshild || m.hasDeathrattle())) retval += m.Angr;

                    }

                    if (enemyhaschillmaw && (m.Hp <= 3 && !m.divineshild && !m.hasDeathrattle())) retval -= m.Angr * 2;
                    if (abomination && (m.Hp <= 2 && !m.divineshild && !m.hasDeathrattle())) retval -= m.Angr * 2;

                    //meta

                    //switch (p.ownHeroName)
                    //{
                    //    case HeroEnum.warlock: break;
                    //    default: break;
                    //}



                    //
                    if (p.ownHeroName == HeroEnum.warlock && (TAG_RACE)m.handcard.card.race == TAG_RACE.DEMON) retval += 0.1f;
                    //if (p.ownHeroName == HeroEnum.warlock && m.name == CardDB.cardName.darkshirecouncilman) retval += 1.5f;

                    if (p.ownHeroName == HeroEnum.hunter && (TAG_RACE)m.handcard.card.race == TAG_RACE.PET) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.shaman && (TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM) retval += 0.2f;
                    if (p.ownHeroName == HeroEnum.pala && m.name == CardDB.cardName.silverhandrecruit) retval += 0.1f; ;
                    if (p.ownHeroName == HeroEnum.mage && (TAG_RACE)m.handcard.card.race == TAG_RACE.MECHANICAL) retval += 0.1f;
                    if (p.ownHeroName == HeroEnum.mage && m.name == CardDB.cardName.flamewaker) retval += 5;

                    
                    if (!hasenemytaunt && !p.enemyHero.immune && m.Ready) retval -= 10 * m.Angr;

                    //minion each
                    if (impgangbossgoodposition && m.handcard.card.name == CardDB.cardName.impgangboss && m.Hp == 4 && p.ownMaxMana <= 3 && m.playedThisTurn) retval += 5;

                    if (m.name == CardDB.cardName.darkshirecouncilman && p.enemyMinions.Count == 0 && p.ownMaxMana <= 3) retval += 5;
                    if (p.enemyMinions.Count >= 1)
                    {
                        if ((m.name == CardDB.cardName.darkshirecouncilman || m.name == CardDB.cardName.tundrarhino) && m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr && m.Angr < p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Hp) retval -= 5;
                        if (m.name == CardDB.cardName.darkshirecouncilman && (m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr + enemypotentialattack + p.enemyWeaponAttack) && p.ownMaxMana <= 3) retval -= 10;
                        if (m.name == CardDB.cardName.tundrarhino && (m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr + enemypotentialattack + p.enemyWeaponAttack) && p.ownMaxMana <= 5) retval -= 10;
                        // 적미니언 공높은놈이랑 비교.. 
                        // 적미니언 공높은놈보다 피 작고 (한방에죽음) + 적 미니언 공높은놈보다 공낮으면 -밸류;
                        if (m.name == CardDB.cardName.flametonguetotem && (m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr + enemypotentialattack + p.enemyWeaponAttack) && p.ownMinions.Find (a => a.taunt) == null) retval -= 10;
                        if (m.name == CardDB.cardName.manatidetotem && (m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr + enemypotentialattack + p.enemyWeaponAttack) && p.ownMinions.Find(a => a.taunt) == null) retval -= 10;
                        if (m.name == CardDB.cardName.wickedwitchdoctor && (m.Hp <= p.searchRandomMinion(p.enemyMinions, Playfield.searchmode.searchHighestAttack).Angr + enemypotentialattack + p.enemyWeaponAttack)) retval -= 5;
                    }

                    if (m.name == CardDB.cardName.scavenginghyena && !m.silenced)
                    {
                        if (m.Angr == 2) retval -= 5; // penalty 2/2 hyena
                        if (m.taunt) retval -= 4; // penalty taunt
                    }

                    if (m.name == CardDB.cardName.tundrarhino && !m.silenced)
                    {
                        retval += 8;
                    }


                    ////attack face when own taunt can control board
                    //if (m.taunt)
                    //{
                    //    foreach (Minion mnn in p.enemyMinions)
                    //    {
                    //        if (mnn.Angr >= m.Hp) retval -= 1.5f * (p.enemyHero.Hp + p.enemyHero.armor);
                    //    }
                    //}

                    //anti aoe
                    //if (m.handcard.card.anti_aoe_minion >= 1 && !m.silenced && p.ownMinions.Count >= 2 && p.enemyMinions.Count == 0) retval += m.handcard.card.anti_aoe_minion * 0.5f;
                    if (m.handcard.card.anti_aoe_minion >= 1 && !m.silenced)
                    {
                        retval += m.handcard.card.anti_aoe_minion;
                        if (m.name == CardDB.cardName.ayablackpaw) retval += m.handcard.card.anti_aoe_minion * (p.anzOwnJadeGolem -1);
                    }

                    if (m.divineshild && p.ownMinions.Count >= 2 && p.enemyMinions.Count == 0) retval += m.Angr * 2 - m.AdjacentAngr * 2;

                    //kill card
                    //if (p.enemyMaxMana >= 5 && Big_Game_Hunter == 0 && m.Angr >= 7) retval -= (m.Angr) * 0.3f; //if enemy not uses bgh, minion value is half.

                    if (p.enemyWeaponName == CardDB.cardName.deathsbite && p.enemyWeaponDurability == 1 && m.Hp == 1)
                    {
                        retval -= 0.5f;
                        retval -= m.Hp * 1;
                        retval -= m.Angr * 2;
                    }
                    retval += 1;


                    retval += m.Hp * 1;
                    if (m.Angr == 0) retval -= m.Hp * 0.3f;
                    //if (m.Hp >= 5) retval += m.Hp * 0.5f;
                    retval += m.Angr * 2;
                    //retval += m.handcard.card.rarity;
                    retval -= m.AdjacentAngr * 2;

                    if (m.Angr == 1 && m.Hp == 1 && !m.divineshild) retval -= 1;



                    bool pirateaggrowarrior = false;
                    if (p.enemyHeroName == HeroEnum.warrior)
                    {
                        int upgrade = 0;
                        int bloodsail_raider = 0;
                        int Bloodsail_Cultist = 0;
                        int southseadeckhand = 0;
                        int Small_Time_Buccanee = 0;
                        int Patches_the_Pirate = 0;
                        int Dread_Corsair = 0;
                        foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
                        {
                            if (e.Key == CardDB.cardIDEnum.EX1_409) upgrade = e.Value;//upgrade EX1_409
                            if (e.Key == CardDB.cardIDEnum.NEW1_018) bloodsail_raider = e.Value;//NEW1_018 //bloodsail raider
                            if (e.Key == CardDB.cardIDEnum.OG_315) Bloodsail_Cultist = e.Value;//OG_315 Bloodsail Cultist
                            if (e.Key == CardDB.cardIDEnum.CS2_146) southseadeckhand = e.Value;//CS2_146 //southseadeckhand
                            if (e.Key == CardDB.cardIDEnum.CFM_325) Small_Time_Buccanee = e.Value;//Small-Time Buccanee
                            if (e.Key == CardDB.cardIDEnum.CFM_637) Patches_the_Pirate = e.Value;//Patches the Pirate
                            if (e.Key == CardDB.cardIDEnum.NEW1_022) Dread_Corsair = e.Value;//Dread Corsair
                        }
                        if (upgrade >= 1 || bloodsail_raider >= 1 || Bloodsail_Cultist >= 1 || southseadeckhand >= 1 || Small_Time_Buccanee >= 1 || Patches_the_Pirate >= 1 || Dread_Corsair >= 1) pirateaggrowarrior = true; //pirate warrior
                    }


                    if (!pirateaggrowarrior)
                    {
                        if (p.enemyHeroName == HeroEnum.warrior && Ravaging_Ghoul == 0 && m.Hp == 1) retval -= m.Angr * 0.5f;
                        else if (p.enemyHeroName == HeroEnum.warrior && Execute == 0 && m.wounded && (m.Angr >= 4 || m.Hp >= 5)) retval -= m.Angr * 0.5f;
                    }
                    

                    if (m.wounded) retval += (m.maxHp - m.Hp) * 0.001f;

                    if (p.enemyWeaponAttack >= 1 && p.enemyWeaponAttack >= m.Hp && !m.hasDeathrattle() && !m.divineshild) retval -= m.Angr * 0.5f;

                    if (m.windfury && p.enemyMinions.Count == 0) retval += m.Angr;

                    if (m.divineshild && p.enemyHeroAblility.card.name != CardDB.cardName.fireblast) retval += m.Angr * 1.5f;
                    if (m.stealth) retval += 1;
                    if (m.taunt)
                    {
                        retval += m.AdjacentAngr * 0.25f;
                        hasowntauntminion = true;
                    }
                    if (m.handcard.card.isSpecialMinion && !m.silenced)
                    {
                        retval += 1;
                        //if (!m.taunt && m.stealth) retval += (m.Angr < 4 ? 2 : 5);
                    }
                    if (m.destroyOnEnemyTurnEnd || m.destroyOnEnemyTurnStart || m.destroyOnOwnTurnEnd || m.destroyOnOwnTurnStart)
                    {
                        retval -= m.Hp + m.Angr * 2 + m.handcard.card.rarity;
                        //Helpfunctions.Instance.ErrorLog("destroyOnEnemyTurnEnd: " + m.name);
                    }

                    //m.shadowmadnessed
                    if (m.shadowmadnessed) retval -= m.Hp - 2 * m.Angr - 5;

                    //zonepos
                    if (m.name == CardDB.cardName.knifejuggler)
                    {
                        if (jugglercount == 1 && m.zonepos == 1) retval += 0.01f;
                        if (jugglercount == 2 && (m.zonepos == 1 || m.zonepos == 2)) retval += 0.01f;
                    }
                    if (m.name == CardDB.cardName.possessedvillager)
                    {
                        if (deathrattlecount == 1 && m.zonepos == p.ownMinions.Count) retval += 0.01f;
                        if (deathrattlecount == 2 && (m.zonepos == p.ownMinions.Count || m.zonepos == p.ownMinions.Count - 1)) retval += 0.01f;
                    }
                    if (m.name == CardDB.cardName.doomguard || m.name == CardDB.cardName.seagiant)
                    {
                        if (jugglercount >= 1 && m.zonepos == jugglercount + 1) retval += 0.01f;
                        if (jugglercount == 0 && m.zonepos == 1) retval += 0.01f;
                    }
                    if ((m.name == CardDB.cardName.direwolfalpha || m.name == CardDB.cardName.flametonguetotem) && !m.silenced)
                    {
                        if (m.zonepos == p.ownMinions.Count || m.zonepos == 1) retval -= 2;
                    }


                }
            }

            //rockbiter
            bool rockbiterHero = p.playactions.Find(a => a.actionType == actionEnum.playcard && a.card.card.name == CardDB.cardName.rockbiterweapon && a.target.entityID == p.ownHero.entityID) != null;
            if (rockbiterHero && p.ownHero.Ready) retval -= 30;
            //if (rockbiterHero) Helpfunctions.Instance.ErrorLog("rockbiterHero " + rockbiterHero + " ");
            bool rockbiterMinion = false;
            foreach (Minion m in p.ownMinions)
            {
                if (p.playactions.Find(a => a.actionType == actionEnum.playcard && a.card.card.name == CardDB.cardName.rockbiterweapon && a.target.entityID == m.entityID) != null) rockbiterMinion = true;
                if (rockbiterMinion && m.Ready) { retval -= 30; break; }
            }
            //if (rockbiterMinion) Helpfunctions.Instance.ErrorLog("rockbiterMinion " + rockbiterMinion + " ");

            //shaman overload

            bool hasThunder = false;
            foreach (Handmanager.Handcard hc in p.owncards)
            {
                if (hc.card.name == CardDB.cardName.thunderbluffvaliant) hasThunder = true;
            }

            if (hasThunder && p.ownMaxMana == 6) retval -= p.owedRecall * 3;



            foreach (Minion m in p.enemyMinions)
            {
                retval -= this.getEnemyMinionValue(m, p);  
            }


            int Houndmaster = 0; //Unleash the Hounds EX1_538
            foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
            {
                if (e.Key == CardDB.cardIDEnum.DS1_070) Houndmaster = e.Value;//Houndmaster DS1_070
            }
            if (Houndmaster == 0 && p.enemyHeroName == HeroEnum.hunter)
            {
                if (p.enemyMinions.Find(a => (TAG_RACE)a.handcard.card.race == TAG_RACE.PET) != null && p.enemyMaxMana >= 3) retval -= 5;
            }


            retval -= 8 * p.enemySecretCount;
            retval += 3 * p.ownSecretsIDList.Count;


            /// SECRETS
            /// 
            foreach (CardDB.cardIDEnum secretID in p.ownSecretsIDList)
            {
                if (secretID == CardDB.cardIDEnum.EX1_136) // redemption
                {
                    List<Minion> temp = new List<Minion>(p.ownMinions);
                    temp.Sort((a, b) => a.Angr.CompareTo(b.Angr));//take the weakest
                    if (temp.Count == 0) break;
                    Minion MINION = temp[0];
                    if (temp.Count == 1 && !p.ownSecretsIDList.Contains(CardDB.cardIDEnum.EX1_130))
                    {
                        int secretvalue = 0;
                        secretvalue ++;
                        switch (MINION.name)
                        {
                            case CardDB.cardName.thesilverhand: secretvalue -= 5; break;
                            case CardDB.cardName.shieldedminibot: secretvalue += 3; break;
                            case CardDB.cardName.sludgebelcher: secretvalue += 5; break;
                            case CardDB.cardName.tirionfordring: secretvalue += 10; break;
                        }
                        retval += secretvalue;
                        //Helpfunctions.Instance.ErrorLog("secretvalue " + secretvalue + " " );
                    }

                }
            }








            //retval -= 2 * p.lostDamage;//damage which was to high (like killing a 2/1 with an 3/3 -> => lostdamage =2
            //retval -= p.lostWeaponDamage;
            //if (p.ownMinions.Count == 0) retval -= 5;

            //if (p.enemyMinions.Count == 0 && p.ownMinions.Count >= 1) retval += 5;
            //if (p.enemyMinions.Count == 0 && p.ownMaxMana <= 4) retval += 5;
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
                    bool ragon = false;
                    foreach (Minion mnn in p.ownMinions)
                    {
                        if (mnn.name == CardDB.cardName.ragnarosthefirelord && !mnn.silenced && (p.enemyHero.Hp + p.enemyHero.armor <= 8))
                        {
                            ragon = true;
                        }
                    }
                    //Helpfunctions.Instance.ErrorLog("ragon " + ragon + " "  );
                    if (ragon)
                    {
                        retval += 1600 - 200 * p.enemyMinions.Count;
                    }
                    else
                    {

                    }
                    retval += 10000;

                }
                else
                {
                    if (p.turnCounter >= 2 && p.enemyMinions.Count == 0)
                    {
                        retval += 15;
                    }
                    
                    //retval += 5;//10000
                    if (p.numPlayerMinionsAtTurnStart == 0) retval += 3; // if we can kill the enemy even after a board clear, bigger bonus
                    if (p.loathebLastTurn > 0) retval += 15;  // give a bonus to turn 2 sims where we played loatheb in turn 1 to protect our lethal board

                }
            }
            else if (p.ownHero.Hp > 0)
            {
                // if our damage on board is lethal, give a strong bonus so enemy AI avoids this outcome in its turn (i.e. AI will clear our minions if it can instead of ignoring them)
                if (p.turnCounter == 1 && p.guessHeroDamage(true) >= p.enemyHero.Hp + p.enemyHero.armor && p.enemyMinions.Count == 0) retval += 5;

                if (p.turnCounter <= 1)
                {
                    int fatiguedamage = (p.enemyDeckSize == 0) ? p.enemyHeroFatigue + 1 : 0;
                    bool ragon = false;
                    foreach (Minion mnn in p.ownMinions)
                    {
                        if (mnn.name == CardDB.cardName.ragnarosthefirelord && !mnn.silenced && (p.enemyHero.Hp + p.enemyHero.armor - fatiguedamage <= 8))
                        {
                            ragon = true;
                        }
                    }
                    //Helpfunctions.Instance.ErrorLog("ragon " + ragon + " "  );
                    if (ragon)
                    {
                        retval += 1600 - 200 * p.enemyMinions.Count;
                    }
                }

            }



            ////soulfire etc
            //int deletecardsAtLast = 0;
            //foreach (Action a in p.playactions)
            //{
            //    if (a.actionType != actionEnum.playcard) continue;
            //    if (a.card.card.name == CardDB.cardName.soulfire || a.card.card.name == CardDB.cardName.doomguard || a.card.card.name == CardDB.cardName.succubus) deletecardsAtLast = 1;
            //    if (deletecardsAtLast == 1 && !(a.card.card.name == CardDB.cardName.soulfire || a.card.card.name == CardDB.cardName.doomguard || a.card.card.name == CardDB.cardName.succubus)) retval -= 20;
            //}





            //do face-attack:
            int lethalmissing = p.enemyHero.Hp + p.enemyHero.armor;
            if (!p.enemyHero.immune)// enemys turn ends -> attack with all minions face (if there is no taunt)
            {
                //Helpfunctions.Instance.ErrorLog(turnCounter + " bef " + p.ownHero.Hp + " " + p.ownHero.armor);
                int attack = 0;
                int directattack = 0;
                int hasenemytaunt = 0;
                if (!p.ownHero.allreadyAttacked && !p.ownHero.frozen && p.enemyWeaponName != CardDB.cardName.foolsbane) attack += p.ownHero.Angr;
                if (p.ownWeaponName == CardDB.cardName.doomhammer && p.ownWeaponDurability >= 2 && !p.ownHero.allreadyAttacked && !p.ownHero.frozen) attack += p.enemyHero.Angr;




                foreach (Minion m in p.enemyMinions)
                {
                    if (m.taunt) attack -= m.Hp;
                }

                foreach (Minion m in p.ownMinions)
                {
                    if (m.name == CardDB.cardName.ragnarosthefirelord && p.enemyMinions.Count == 0 && !m.silenced) directattack += 8;
                    if (m.Ready) attack += m.Angr;
                    if (m.Ready && m.windfury && m.numAttacksThisTurn == 0) attack += m.Angr;

                }

                switch (p.ownHeroAblility.card.name)
                {
                    case CardDB.cardName.steadyshot: directattack += 2; break; //hunter
                    case CardDB.cardName.ballistashot: directattack += 3; break; //hunter 3 damage
                    case CardDB.cardName.daggermastery: if (p.ownWeaponAttack == 0) attack += 1; break; //rogue
                    case CardDB.cardName.poisoneddaggers: if (p.ownWeaponAttack == 0) attack += 2; break; //rogue 2 att
                    case CardDB.cardName.shapeshift: attack += 1; break; //druid
                    case CardDB.cardName.direshapeshift: attack += 2; break; //druid 2att
                    case CardDB.cardName.fireblast: directattack += 1; break; //mage
                    case CardDB.cardName.fireblastrank2: directattack += 2; break; //mage 2att
                    case CardDB.cardName.mindspike: directattack += 2; break; //dark priest
                    case CardDB.cardName.mindshatter: directattack += 3; break; //dark priest rank2
                    case CardDB.cardName.lightningjolt: directattack += 2; break; //2dmg shamman weapon
                    default: break;
                }

                lethalmissing -= attack + directattack;
                //Helpfunctions.Instance.ErrorLog("lethalmissing " + lethalmissing + " " + retval);
            }

            if (p.owncards.Count <= 9 && p.ownDeckSize >= 1)
            {
                if (lethalmissing <= 5 && !p.enemyHero.immune) retval += p.owncarddraw * 2;
                if (lethalmissing <= 4 && p.ownMinions.Count >= 1 && !p.enemyHero.immune) retval += p.owncarddraw * 3;
                if (lethalmissing <= 2 && p.ownMinions.Count >= 1 && !p.enemyHero.immune) retval += p.owncarddraw * 5;
            }

            


            this.lethalMissed = lethalmissing;









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
                            if (hastauntminion[i-1].name == m.name)
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
                //        Helpfunctions.Instance.ErrorLog(" mnn 카운트" + hastauntminion.Count + "\r\n");
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
                            if (mnn.Hp == enemyminionsattack[i-1].Angr || mnn.Hp == tempEnemyweaponattack)
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
                        } break; 
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


            if ((concedevalue <= 0 && p.turnCounter >= 1 && p.enemyHero.Hp >= 1 && hasowntaunthp <= potentialenemyattack) || directenemyheropowerattack >= p.ownHero.Hp + p.ownHero.armor)
            {
                //worst case: we die on own turn
                
                foreach (Minion m in p.ownMinions)
                {
                    if (m.name == CardDB.cardName.manatidetotem && !m.silenced) p.owncarddraw -= 1;
                }

                if (p.owncarddraw >= 1) retval += 4500;
                if (p.enemyHero.immune) retval += 4800;
                retval -= 5000;

            }
            else
            {
                if (concedevalue <= 6) retval -= 10 * (7 - concedevalue);
            }
            //Helpfunctions.Instance.ErrorLog("concedevalue " + concedevalue + " " + retval);
            //Helpfunctions.Instance.ErrorLog("potentialenemyattack / hasowntaunthp  : " + potentialenemyattack + "/" + hasowntaunthp);
            //Helpfunctions.Instance.ErrorLog("hasowntaunthp " + hasowntaunthp + " ");
            if (p.ownDeckSize == 0)
            {
                if (p.enemyDeckSize + p.enemyCardsCountStarted >= 5) retval -= 600;
                if (p.enemyMinions.Count >= p.ownMinions.Count) retval -= 600;
                if (p.enemyHero.Hp + p.enemyHero.armor >= p.ownHero.Hp + p.ownHero.armor) retval -= 700;
            }


            if (p.enemyHero.immune && p.enemyMinions.Find(a => a.name == CardDB.cardName.violetillusionist && !a.silenced) == null)
            {
                retval += 50;
            }


            //if (p.enemyHero.Hp >= 1 && p.ownHero.Hp <= 0)
            //{
            //    //Helpfunctions.Instance.ErrorLog("turncounter " + p.turnCounter + " " + retval);

            //    if (p.turnCounter == 0 || concedevalue <= 0) // own turn 
            //    {
            //        //worst case: we die on own turn
            //        retval += p.owncarddraw * 100;
            //        retval = -10000;
            //    }
            //    else
            //    {
            //        if (p.turnCounter == 1) // enemys first turn
            //        {
            //            //retval += p.owncarddraw * 500;
            //            retval -= 1000;
            //        }
            //        if (p.turnCounter >= 2)
            //        {
            //            //carddraw next turn doesnt count this turn :D
            //            retval -= 100;
            //            if (p.loathebLastTurn > 0) retval += 50;
            //        }
            //    }
            //}

            //if (p.ownHero.Hp <= 0 && p.turnCounter < 2) retval = -10000;

            // give a bonus for making the enemy spend more mana dealing with our board, so boards where the enemy makes different plays
            // aren't considered as equal value (i.e. attacking the enemy and making him spend mana to heal vs not attacking at all)
            if (p.turnCounter == 1 || p.turnCounter == 3) retval += p.enemyMaxMana - p.mana;

            p.value = retval;
            //Helpfunctions.Instance.ErrorLog("retval" + retval);
            return retval;
        }

        //other value of the board for enemys turn? (currently the same as getplayfield value)
        public override float getPlayfieldValueEnemy(Playfield p)
        {
            return getPlayfieldValue(p);
        }


        public override float getEnemyMinionValue(Minion m, Playfield p)
        {
            float retval = 1;

            if (m.handcard.card.isSpecialMinion && !m.silenced)
            {
                retval += 1;
            }

            if (p.enemyHeroName == HeroEnum.warrior || p.enemyHeroName == HeroEnum.thief || p.enemyHeroName == HeroEnum.shaman)
            {
                int upgrade = 0;
                int bloodsail_raider = 0;
                int Bloodsail_Cultist = 0;
                int southseadeckhand = 0;
                int Small_Time_Buccanee = 0;
                int Patches_the_Pirate = 0;
                int Dread_Corsair = 0;
                int Alexstraszas_Champion = 0;
                foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
                {
                    if (e.Key == CardDB.cardIDEnum.EX1_409) upgrade = e.Value;//upgrade EX1_409
                    if (e.Key == CardDB.cardIDEnum.NEW1_018) bloodsail_raider = e.Value;//NEW1_018 //bloodsail raider
                    if (e.Key == CardDB.cardIDEnum.OG_315) Bloodsail_Cultist = e.Value;//OG_315 Bloodsail Cultist
                    if (e.Key == CardDB.cardIDEnum.CS2_146) southseadeckhand = e.Value;//CS2_146 //southseadeckhand
                    if (e.Key == CardDB.cardIDEnum.CFM_325) Small_Time_Buccanee = e.Value;//Small-Time Buccanee
                    if (e.Key == CardDB.cardIDEnum.CFM_637) Patches_the_Pirate = e.Value;//Patches the Pirate
                    if (e.Key == CardDB.cardIDEnum.NEW1_022) Dread_Corsair = e.Value;//Dread Corsair
                    if (e.Key == CardDB.cardIDEnum.AT_071) Alexstraszas_Champion = e.Value;//Alexstrasza's Champion AT_071
                }

                bool aggrodeck = false;
                foreach (Minion mm in p.enemyMinions)
                {
                    if (mm.name == CardDB.cardName.bloodsailraider || 
                        mm.name == CardDB.cardName.bloodsailcultist || 
                        mm.name == CardDB.cardName.southseadeckhand || 
                        mm.name == CardDB.cardName.smalltimebuccaneer || 
                        mm.name == CardDB.cardName.patchesthepirate ||
                        mm.name == CardDB.cardName.dreadcorsair ||
                        mm.name == CardDB.cardName.alexstraszaschampion)
                    {
                        aggrodeck = true;
                    }
                }

                if (aggrodeck || upgrade >= 1 || bloodsail_raider >= 1 || Bloodsail_Cultist >= 1 || southseadeckhand >= 1 || Small_Time_Buccanee >= 1 || Patches_the_Pirate >= 1 || Dread_Corsair >= 1 || Alexstraszas_Champion >= 1) retval += 2 * m.Angr; //pirate warrior
            }

            if (p.enemyHeroName == HeroEnum.priest)
            {
                int Kabal_Talonpriest = 0;
                foreach (KeyValuePair<CardDB.cardIDEnum, int> e in Probabilitymaker.Instance.enemyGraveyard)
                {
                    if (e.Key == CardDB.cardIDEnum.CFM_626) Kabal_Talonpriest = e.Value;//upgrade EX1_409
                }
                if (Kabal_Talonpriest == 0) retval += 4.5f;
            }

            if (p.enemyHeroName == HeroEnum.hunter)
            {
                if (m.name == CardDB.cardName.tundrarhino) retval += (p.enemyMaxMana) * 3;
                retval += 2;
            }

            if (p.enemyHeroName == HeroEnum.druid || p.enemyHeroName == HeroEnum.warlock)
            {
                if (m.divineshild) retval += m.Angr * 2;
                if (p.enemyHeroName == HeroEnum.warlock) retval += 2; //zoo 
                retval += 2; // token , zoo
                if (p.enemyHeroName == HeroEnum.warlock)
                {
                    if (m.name == CardDB.cardName.malchezaarsimp && !m.silenced) retval += 3;
                }
            }

            if (p.enemyHeroName == HeroEnum.shaman)
            {
                if (m.divineshild) retval += m.Angr * 2;
                if ((TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM) retval += 2; //totemic shaman
                if (p.ownMaxMana <= 5 && (TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM) retval += 2;
                if ((TAG_RACE)m.handcard.card.race == TAG_RACE.TOTEM && m.Hp >= 3) retval += 2; //totemic shaman
                if (m.name == CardDB.cardName.manatidetotem && !m.silenced) retval += 3;
                retval += m.handcard.card.cost * 0.5f; //evolve
                retval += 4;
            }
            
            retval += m.Hp * 1;
            if (m.wounded) retval += (m.maxHp - m.Hp) * 0.001f;
            if (m.name == CardDB.cardName.doomsayer && m.Angr == 0 && m.silenced) retval -= m.Hp * 2;
            if (!m.frozen && !((m.name == CardDB.cardName.ancientwatcher) && !m.silenced))
            {
                retval += m.Angr * 2;
                if (m.windfury) retval += m.Angr * 2;
                if (p.enemyHero.immune) retval += m.Angr;
                //if (m.Angr >= 5) retval += m.Angr - 2;
                //if (m.Angr >= 7) retval += m.Angr - 2;
            }

            if (m.name == CardDB.cardName.sylvanaswindrunner && !m.silenced) retval += 10;
            if (m.name == CardDB.cardName.acolyteofpain && !m.silenced && m.Hp >= 2) retval += 12;
            if (m.name == CardDB.cardName.acolyteofpain && !m.silenced && m.Hp == 1) retval += 15;
            if (m.name == CardDB.cardName.impgangboss && !m.silenced) retval += 5;
            if (m.name == CardDB.cardName.flamewaker && !m.silenced) retval += 4;
            if (m.name == CardDB.cardName.gadgetzanauctioneer && !m.silenced) retval += 10;
            if (m.name == CardDB.cardName.flametonguetotem && !m.silenced && p.enemyMinions.Count == 1) retval -= 6;

            if (m.name == CardDB.cardName.ragnarosthefirelord && !m.silenced && p.ownHero.Hp + p.ownHero.armor <= 8) retval += 30;


            if (m.spellpower >= 1) retval += m.spellpower * 5;
            if (m.spellpower >= 1 && p.enemyHeroName == HeroEnum.mage) retval += m.spellpower * 5;
            if (m.taunt) retval+= m.Angr;
            if (m.divineshild) retval += m.Angr * 3;
            //if (m.divineshild && m.Angr == 1 && p.enemyHeroName == HeroEnum.pala) retval += 5;
            if (m.frozen) retval -= 1; // because its bad for enemy :D
            if (m.poisonous) retval += 4;
            //retval += m.handcard.card.rarity;
            //
            if (p.enemyHeroName == HeroEnum.hunter && p.ownHero.Hp <= 12) retval += m.Angr;
            if (p.enemyHeroName == HeroEnum.warlock && (TAG_RACE)m.handcard.card.race == TAG_RACE.DEMON) retval += 0.1f;
            if (p.enemyHeroName == HeroEnum.hunter && (TAG_RACE)m.handcard.card.race == TAG_RACE.PET)
            {
                retval += 0.1f;
                //if (Houndmaster == 0 && p.enemyMaxMana >= 3) retval += 3;
            }
            
            //if (p.enemyHeroName == HeroEnum.shaman && m.name == CardDB.cardName.tunneltrogg && !m.silenced) retval += 5.5f;
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
            if (m.destroyOnEnemyTurnEnd || m.destroyOnEnemyTurnStart || m.destroyOnOwnTurnEnd || m.destroyOnOwnTurnStart) retval -= m.Angr * 2 + m.Hp + m.handcard.card.cost;





            //anti aoe
            if (m.handcard.card.anti_aoe_minion >= 1)  retval += m.handcard.card.anti_aoe_minion * 0.2f;


            //buff
            if (m.enemyBlessingOfWisdom >= 1) retval += 8;
            if (m.name == CardDB.cardName.smalltimebuccaneer && p.enemyWeaponDurability == 0) retval += 4;



            int HighestTauntHp = 0;
            int TauntAttack = 0;
            foreach (Minion mnn in p.ownMinions)
            {
                if (mnn.taunt)
                {
                    if (mnn.Hp > HighestTauntHp) 
                    {
                        HighestTauntHp = mnn.Hp;
                        TauntAttack = mnn.Angr;
                    }
                }               
            }

            bool enemyCankillTaunt = false;
            int SumEnemyMinionAttack = 0;
            foreach (Minion mnn in p.enemyMinions)
            {
                if (mnn.Angr >= HighestTauntHp) enemyCankillTaunt = true;
                SumEnemyMinionAttack += mnn.Angr;
            }
            if (SumEnemyMinionAttack >= HighestTauntHp) enemyCankillTaunt = true;
            if (p.enemyWeaponAttack >= HighestTauntHp) enemyCankillTaunt = true;

            //Helpfunctions.Instance.ErrorLog("enemyCankillTaunt " + enemyCankillTaunt);
            //Helpfunctions.Instance.ErrorLog("" + retval);
            if (!enemyCankillTaunt && HighestTauntHp >= 1 && m.Hp <= TauntAttack)
            {
                switch (p.enemyHeroName)
                {
                    case HeroEnum.hunter: if ((TAG_RACE)m.handcard.card.race != TAG_RACE.PET) retval -= m.Angr / 2; break;
                    case HeroEnum.shaman: break;
                    case HeroEnum.druid: break;
                    case HeroEnum.pala: break;
                    case HeroEnum.priest: break;
                    case HeroEnum.warrior: retval -= m.Angr * 0.5f; break;
                    case HeroEnum.warlock: break;
                    case HeroEnum.thief: retval -= m.Angr * 0.5f; break;
                    case HeroEnum.mage: retval -= m.Angr * 0.5f; break;
                    default: retval -= m.Angr * 0.5f; break;
                }
                retval -= m.Angr * 0.5f;
            }
            //Helpfunctions.Instance.ErrorLog("" + retval + "\r\n");

            return retval;
        }


    }

}