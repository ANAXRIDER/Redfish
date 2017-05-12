using System;
using System.Collections.Generic;

namespace HREngine.Bots
{

    public class Discovery
    {

        string choicenminion = "";
        public int choicebonus;
        class discoveryitem
        {
            public CardDB.cardIDEnum cardid = CardDB.cardIDEnum.None;
            public int bonus;
            public string ownclass = "";
            public string enemyclass = "";

            public discoveryitem(string line)
            {
                this.cardid = CardDB.Instance.cardIdstringToEnum(line.Split(',')[0]);
                this.bonus = Convert.ToInt32(line.Split(';')[0].Split(',')[1]);
                this.ownclass = line.Split(';')[1];
                this.enemyclass = line.Split(';')[2];
            }

        }

        private string ownClass = Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.heroname);
        private string deckName = Hrtprozis.Instance.deckName;
        private string cleanPath = "";

        private List<discoveryitem> discoverylist = new List<discoveryitem>();

        private static Discovery instance;

        public static Discovery Instance
        {
            get
            {
                return instance ?? (instance = new Discovery());
            }
        }

        public void updateInstance()
        {
            ownClass = Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.heroname);
            deckName = Hrtprozis.Instance.deckName;
            lock (instance)
            {
                readCombos();
            }
        }

        public void loggCleanPath()
        {
            Helpfunctions.Instance.logg(cleanPath);
        }

        private void readCombos()
        {
            string[] lines = new string[] { };
            this.discoverylist.Clear();

            string path = Settings.Instance.path;
            string cleanpath = "Silverfish" + System.IO.Path.DirectorySeparatorChar;
            string datapath = path + "Data" + System.IO.Path.DirectorySeparatorChar;
            string cleandatapath = cleanpath + "Data" + System.IO.Path.DirectorySeparatorChar;
            string classpath = datapath + ownClass + System.IO.Path.DirectorySeparatorChar;
            string cleanclasspath = cleandatapath + ownClass + System.IO.Path.DirectorySeparatorChar;
            string deckpath = classpath + deckName + System.IO.Path.DirectorySeparatorChar;
            string cleandeckpath = cleanclasspath + deckName + System.IO.Path.DirectorySeparatorChar;
            const string filestring = "_discovery.txt";


            if (deckName != "" && System.IO.File.Exists(deckpath + filestring))
            {
                path = deckpath;
                cleanPath = cleandeckpath + filestring;
            }
            else if (deckName != "" && System.IO.File.Exists(classpath + filestring))
            {
                path = classpath;
                cleanPath = cleanclasspath + filestring;
            }
            else if (deckName != "" && System.IO.File.Exists(datapath + filestring))
            {
                path = datapath;
                cleanPath = cleandatapath + filestring;
            }
            else if (System.IO.File.Exists(path + filestring))
            {
                cleanPath = cleanpath + filestring;
            }
            else
            {
                Helpfunctions.Instance.ErrorLog("[Discovery] cant find base _discovery.txt, consider creating one");
                return;
            }
            Helpfunctions.Instance.ErrorLog("[Discovery] read " + cleanPath);


            try
            {
                lines = System.IO.File.ReadAllLines(path + "_discovery.txt");
            }
            catch
            {
                Helpfunctions.Instance.ErrorLog("_discovery.txt read error. Continuing without user-defined rules.");
                return;
            }
            
            foreach (string line in lines)
            {
                string shortline = line.Replace(" ", "");
                if (shortline.StartsWith("//")) continue;
                if (shortline.Length == 0) continue;

                try
                {
                    discoveryitem d = new discoveryitem(line);
                    this.discoverylist.Add(d);
                }
                catch
                {
                    Helpfunctions.Instance.ErrorLog("[Discovery] cant read line: " + line);
                }
            }
            Helpfunctions.Instance.ErrorLog("[Discovery] " + discoverylist.Count + " rules found");
        }

        public int getBonusValue(CardDB.cardIDEnum cardid, string ownclass, string enemyclass)
        {
            int bonus = 0;
            foreach (discoveryitem di in this.discoverylist)
            {
                if (di.cardid == cardid && (di.ownclass == "all" || di.ownclass == ownclass) && (di.enemyclass == "all" || di.enemyclass == enemyclass))
                {
                    if (di.bonus > bonus) bonus = di.bonus;
                }
            }

            return bonus;
        }

        public int getChoice(Playfield p)
        {
            this.choicebonus = 0;
            int i = 0;
            int choice = 0;
            int prevbonus = 0;

            bool cankillmortalcoil = false;
            bool enemyhastaunt = false;
            bool enemyhas1hp = false;
            bool soulfiretarget = false;
            bool corruptiontarget = false;
            foreach (Minion mnn in p.enemyMinions)
            {
                if (mnn.Hp <= 1 + p.spellpower) cankillmortalcoil = true;
                if (mnn.taunt) enemyhastaunt = true;
                if (mnn.Hp == 1) enemyhas1hp = true;
                if (mnn.Angr >= 3 && mnn.Hp <= 4 || PenalityManager.Instance.priorityTargets.ContainsKey(mnn.name)) soulfiretarget = true;
                if (mnn.Hp >= 5 || mnn.Angr >= 4) corruptiontarget = true;
            }
            bool canibuff = false;
            foreach (Minion mnn in p.ownMinions)
            {
                if (mnn.Ready) canibuff = true;
            }

            bool hasMinusValue = false;
            List<int> minusvalue = new List<int>(new int[] { 1, 2, 3 });
            foreach (Handmanager.Handcard hc in Handmanager.Instance.handcardchoices)
            {
                CardDB.Card c = hc.card;
                i++;

                int bonus = getBonusValue(c.cardIDenum, Hrtprozis.heroEnumtoName(p.ownHeroName), Hrtprozis.heroEnumtoName(p.enemyHeroName));

                switch (hc.card.name)
                {
                    case CardDB.cardName.mortalcoil:
                        {
                            if (p.mana >= 1 && cankillmortalcoil) bonus += 200; 
                            break;
                        }
                    case CardDB.cardName.bloodsailcorsair:
                        {
                            if (p.mana >= 1 && p.enemyWeaponAttack >= 1) bonus += 200;
                            break;
                        }
                    case CardDB.cardName.reliquaryseeker:
                        {
                            if (p.mana >= 1 && p.ownMinions.Count >= 6) bonus += 200;
                            break;
                        }
                    case CardDB.cardName.elvenarcher:
                        {
                            if (p.mana >= 1 && (enemyhas1hp || Ai.Instance.lethalMissing == 1)) bonus += 250;
                            else if (p.mana >= 1 && cankillmortalcoil) bonus += 200;
                            break;
                        }
                    case CardDB.cardName.stonetuskboar:
                        {
                            if (p.mana >= 1 && (enemyhas1hp || (Ai.Instance.lethalMissing == 1 && !enemyhastaunt))) bonus += 250;
                            break;
                        }
                    case CardDB.cardName.soulfire:
                        {
                            if (p.mana >= 1 && (soulfiretarget || Ai.Instance.lethalMissing <= 4)) bonus += 250;
                            break;
                        }
                    case CardDB.cardName.poweroverwhelming:
                        {
                            if (p.mana >= 1 && ((canibuff && Ai.Instance.lethalMissing <= 4) || (canibuff && corruptiontarget))) bonus += 250;
                            break;
                        }
                    case CardDB.cardName.corruption:
                        {
                            if (p.mana >= 1 && corruptiontarget && !canibuff) bonus += 150;
                            break;
                        }
                    case CardDB.cardName.voidwalker:
                        {
                            if (p.mana >= 1 && Ai.Instance.bestmoveValue <= -400) bonus += 200;
                            break;
                        }
                    case CardDB.cardName.shieldbearer:
                        {
                            if (p.mana >= 1 && Ai.Instance.bestmoveValue <= -400) bonus += 190;
                            break;
                        }
                    case CardDB.cardName.goldshirefootman:
                        {
                            if (p.mana >= 1 && Ai.Instance.bestmoveValue <= -400) bonus += 180;
                            break;
                        }
                    case CardDB.cardName.tournamentattendee:
                        {
                            if (p.mana >= 1 && Ai.Instance.bestmoveValue <= -400) bonus += 180;
                            break;
                        }
                    case CardDB.cardName.sirfinleymrrgglton:
                        {
                            if (p.ownHeroName == HeroEnum.warlock) { bonus -= 100; hasMinusValue = true; minusvalue.Remove(i); }
                            break;
                        }
                    default: break;

                }

                //adaptcards
                //switch (hc.card.cardIDenum)
                //{
                //    case CardDB.cardIDEnum.UNG_999t3: bonus = 250; break;// 3attack
                //    case CardDB.cardIDEnum.UNG_999t8: bonus = 180; break; //divine shield
                //    case CardDB.cardIDEnum.UNG_999t13: //poisonous
                //        {
                //            if (p.enemyMinions.Find(a => a.Hp >= 5) != null) bonus += 100; //very huge
                //            else bonus += 170;
                //            break;
                //        }
                //    case CardDB.cardIDEnum.UNG_999t14: bonus = 150; break;// +1/+1
                //    case CardDB.cardIDEnum.UNG_999t4: bonus = 140; break;// 3hp
                //    case CardDB.cardIDEnum.UNG_999t2: bonus = 100; break;//deathrattle 2 1/1 plants
                                                                  
                //    case CardDB.cardIDEnum.UNG_999t7:
                //        {
                //            bonus = 90;
                //            if (p.guessEnemyHeroLethalMissing() <= 3) bonus += 150;
                //        }//windfury
                //        break;
                //    case CardDB.cardIDEnum.UNG_999t6: //taunt
                //        {
                //            if (Ai.Instance.bestmoveValue <= -400) bonus += 1000;
                //            else if (p.ownHero.Hp <= 10) bonus += 205;
                //            break;
                //        }
                //    case CardDB.cardIDEnum.UNG_999t10: bonus += 1; break;//stealth
                //    case CardDB.cardIDEnum.UNG_999t5: break; //can't be targeted
                //    default: break;
                //}


                if (p.owncards.Count == 0)
                {
                    if (hc.card.cost <= p.mana && p.turnCounter == 0) bonus += 10 * hc.card.cost;
                }
                else if (hc.card.cost == p.ownMaxMana + 1 && p.turnCounter == 0)
                {
                    bonus += 1; //bonus for next turn
                }



                if (bonus > prevbonus)
                {
                    choice = i;
                    prevbonus = bonus;
                }



                Helpfunctions.Instance.ErrorLog("card : " + i + " name : " + hc.card.name);
                Helpfunctions.Instance.logg("card : " + i + " name : " + hc.card.name);
            }

            this.choicebonus = prevbonus;
            

            if (choicebonus == 0 && hasMinusValue)
            {
                if (minusvalue.Count >= 1)
                {
                    choice = minusvalue[0];
                }
            }

            Helpfunctions.Instance.ErrorLog("Choice : " + choice + " bonus : " + choicebonus);
            Helpfunctions.Instance.logg("Choice : " + choice + " bonus : " + choicebonus);
            return choice;
        }
    }
}