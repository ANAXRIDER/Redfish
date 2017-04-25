using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using HSRangerLib;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HREngine.Bots
{

    public static class SilverFishBotPath
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path) + System.IO.Path.DirectorySeparatorChar;
            }
        }

        public static  string SettingsPath
        {
            get{
                string temp = AssemblyDirectory + System.IO.Path.DirectorySeparatorChar + "Common" + System.IO.Path.DirectorySeparatorChar;
                if (System.IO.Directory.Exists(temp) == false)
                {
                    System.IO.Directory.CreateDirectory(temp);
                }

                return temp;
            }
        }

        public static string LogPath
        {
            get
            {
                string temp = AssemblyDirectory + System.IO.Path.DirectorySeparatorChar + "Logs" + System.IO.Path.DirectorySeparatorChar;
                if (System.IO.Directory.Exists(temp) == false)
                {
                    System.IO.Directory.CreateDirectory(temp);
                }

                return temp;
            }
        }
    }

    public class Bot : BotBase
    {
        private static Bot instance;

        public static Bot Instance
        {
            get
            {
                return instance ?? (instance = new Bot());
            }
        }
        public override string Description
        {
            get
            {
                
                return "Silverfish A.I. version V" + Silverfish.Instance.versionnumber +" )\r\n" +
                       "\r\n\r\n\r\n\r\n\r\ni hope you dont see the following version number :P"
                       ;

            }
        }

        public bool doMultipleThingsAtATime = true;
        public int dontmultiactioncount = 0;
        public int POWERFULSINGLEACTION = 0;

        //private int stopAfterWins = 30;
        private int concedeLvl = 5; // the rank, till you want to concede
        DateTime starttime = DateTime.Now;
        Silverfish sf;

        public Behavior behave = new BehaviorControl();

        //stuff for attack queueing :D
        public int numExecsReceived = 0;
        public int numActionsSent = 0;
        public bool shouldSendActions = true;
        public List<Playfield> queuedMoveGuesses = new List<Playfield>();
        
        private bool deckChanged = false;
        private bool shouldSendFakeAction = false;

        //
        bool isgoingtoconcede = false;
        int wins = 0;
        int loses = 0;

        public Bot()
        {

            //it's very important to set HasBestMoveAI property to true
            //or Hearthranger will never call OnQueryBestMove !
            base.HasBestMoveAI = true;

            starttime = DateTime.Now;

            Settings set = Settings.Instance;
            this.sf = Silverfish.Instance;
            behave = set.behave;
            sf.setnewLoggFile();
            CardDB cdb = CardDB.Instance;
            if (cdb.installedWrong)
            {
                Helpfunctions.Instance.ErrorLog("cant find CardDB");
                return;
            }

            bool teststuff = false; // set to true, to run a testfile (requires test.txt file in folder where _cardDB.txt file is located)
            bool printstuff = false; // if true, the best board of the tested file is printet stepp by stepp

            Helpfunctions.Instance.ErrorLog("----------------------------");
            Helpfunctions.Instance.ErrorLog("you are now running uai V" + sf.versionnumber);
            Helpfunctions.Instance.ErrorLog("----------------------------");
            //Helpfunctions.Instance.ErrorLog("test... " + Settings.Instance.logpath + Settings.Instance.logfile);
            if (set.useExternalProcess) Helpfunctions.Instance.ErrorLog("YOU USE SILVER.EXE FOR CALCULATION, MAKE SURE YOU STARTED IT!");
            if (set.useExternalProcess) Helpfunctions.Instance.ErrorLog("SILVER.EXE IS LOCATED IN: " + Settings.Instance.path);
            
            if (!sf.startedexe && set.useExternalProcess && (!set.useNetwork || (set.useNetwork && set.netAddress == "127.0.0.1")))
            {
                sf.startedexe = true;
                Task.Run(() => startExeAsync());
            }


            if (teststuff)//run autotester for developpers
            {
                Ai.Instance.autoTester(printstuff);
            }

            this.doMultipleThingsAtATime = Settings.Instance.speedy;

            this.doMultipleThingsAtATime = true; // for easier debugging+bug fixing in the first weeks after update
            //will be false until xytrix fixes it (@xytrix end the action list, after playing a tracking/discover card)
        }

        private void startExeAsync()
        {
            System.Diagnostics.Process[] pname = System.Diagnostics.Process.GetProcessesByName("Redfish");
            string directory = Settings.Instance.path + "Redfish.exe";
            bool hasToOpen = true;

            if (pname.Length >= 1)
            {

                for (int i = 0; i < pname.Length; i++)
                {

                    string fullPath = pname[i].Modules[0].FileName;
                    if (fullPath == directory) hasToOpen = false;
                }
            }

            if (hasToOpen)
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(directory);
                startInfo.WorkingDirectory = Settings.Instance.path;
                System.Diagnostics.Process.Start(startInfo);
            }

            sf.startedexe = false; //reset it in case user closes exe
        }

        /// <summary>
        /// HRanger Code
        /// invoke when game enter mulligan
        /// </summary>
        /// <param name="e">
        ///     e.card_list -- mulligan card list
        ///     e.replace_list -- toggle card list (output)
        /// </param>
        public override void OnGameMulligan(GameMulliganEventArgs e)
        {
            if (e.handled || e.card_list.Count == 0) // if count==0 then HR is conceding
            {
                return;
            }

            //set e.handled to true, 
            //then bot will toggle cards by e.replace_list 
            //and will not use internal mulligan logic anymore.
            e.handled = true;

            if (Settings.Instance.learnmode)
            {
                e.handled = false;
                return;
            }

            var list = e.card_list;

            Entity enemyPlayer = base.EnemyHero;
            Entity ownPlayer = base.FriendHero;
            string enemName = Hrtprozis.Instance.heroIDtoName(enemyPlayer.CardId);
            string ownName = Hrtprozis.Instance.heroIDtoName(ownPlayer.CardId);

            // reload settings
            HeroEnum heroname = Hrtprozis.Instance.heroNametoEnum(ownName);
            HeroEnum enemyHeroname = Hrtprozis.Instance.heroNametoEnum(enemName);
            if (deckChanged || heroname != Hrtprozis.Instance.heroname)
            {
                if (heroname != Hrtprozis.Instance.heroname)
                {
                    Helpfunctions.Instance.ErrorLog("New Class: \"" + Hrtprozis.Instance.heroEnumtoCommonName(heroname) + "\", Old Class: \"" + Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.heroname) + "\"");
                }
                Hrtprozis.Instance.setHeroName(ownName);
                ComboBreaker.Instance.updateInstance();
                Discovery.Instance.updateInstance();
                Mulligan.Instance.updateInstance();
                deckChanged = false;
            }
            if (deckChanged || heroname != Hrtprozis.Instance.heroname || enemyHeroname != Hrtprozis.Instance.enemyHeroname)
            {
                Hrtprozis.Instance.setEnemyHeroName(enemName);
                if (enemyHeroname != Hrtprozis.Instance.enemyHeroname)
                {
                    Helpfunctions.Instance.ErrorLog("New Enemy Class: \"" + Hrtprozis.Instance.heroEnumtoCommonName(enemyHeroname) + "\", Old Class: \"" + Hrtprozis.Instance.heroEnumtoCommonName(Hrtprozis.Instance.enemyHeroname) + "\"");
                }

                behave = Settings.Instance.updateInstance();
            }
            
            sf.setnewLoggFile();
            Settings.Instance.loggCleanPath();
            Mulligan.Instance.loggCleanPath();
            Discovery.Instance.loggCleanPath();
            ComboBreaker.Instance.loggCleanPath();



            if (Hrtprozis.Instance.startDeck.Count > 0)
            {
                string deckcards = "Deck: ";
                foreach (KeyValuePair<CardDB.cardIDEnum, int> card in Hrtprozis.Instance.startDeck)
                {
                    deckcards += card.Key;
                    if (card.Value > 1) deckcards += "," + card.Value;
                    deckcards += ";";
                }
                Helpfunctions.Instance.logg(deckcards);
            }

            //reload external process settings too
            Helpfunctions.Instance.resetBuffer();
            Helpfunctions.Instance.writeToBuffer(Hrtprozis.Instance.deckName + ";" + ownName + ";" + enemName + ";");
            Helpfunctions.Instance.writeBufferToDeckFile();

            if (Mulligan.Instance.hasmulliganrules(ownName, enemName))
            {
                bool hascoin = false;
                List<Mulligan.CardIDEntity> celist = new List<Mulligan.CardIDEntity>();

                foreach (var item in list)
                {
                    Helpfunctions.Instance.ErrorLog("cards on hand for mulligan: " + item.CardId);
                    if (item.CardId != "GAME_005")// dont mulligan coin
                    {
                        celist.Add(new Mulligan.CardIDEntity(item.CardId, item.EntityId));
                    }
                    else
                    {
                        hascoin = true;
                    }

                }
                if (celist.Count >= 4) hascoin = true;
                List<int> mullentities = Mulligan.Instance.whatShouldIMulligan(celist, ownName, enemName, hascoin);
                foreach (var item in list)
                {
                    if (mullentities.Contains(item.EntityId))
                    {
                        Helpfunctions.Instance.ErrorLog("Rejecting Mulligan Card " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(item.CardId) + " because of your rules");
                        //toggle this card
                        e.replace_list.Add(item);
                    }
                }

            }
            else
            {
                foreach (var item in list)
                {
                    if (item.Cost >= 4)
                    {
                        Helpfunctions.Instance.ErrorLog("Rejecting Mulligan Card " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(item.CardId) + " because it cost is >= 4.");

                        e.replace_list.Add(item);

                    }
                    if (item.CardId == "EX1_308" || item.CardId == "EX1_622" || item.CardId == "EX1_005")
                    {
                        Helpfunctions.Instance.ErrorLog("Rejecting Mulligan Card " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(item.CardId) + " because it is soulfire or shadow word: death");
                        e.replace_list.Add(item);
                    }
                }
            }

            Ai.Instance.bestmoveValue = 0; // not concede
            //Helpfunctions.Instance.logg("Ai.Instance.bestmoveValue " + Ai.Instance.bestmoveValue);

            if (Mulligan.Instance.loserLoserLoser)
            {
                if (!autoconcede())
                {
                    concedeVSenemy(ownName, enemName);
                }

                //set concede flag
                e.concede = this.isgoingtoconcede;
            }
        }

        /// <summary>
        /// invoke when drafting arena cards (including hero draft)
        /// </summary>
        /// <param name="e"></param>
        public override void OnGameArenaDraft(GameArenaDraftEventArgs e)
        {
            //must set e.handled to true if you handle draft in this function.
            e.handled = false;


            //if (e.is_hero_choices)
            //{
            //    //choose hero here
            //    e.draft_pick_id = GetBestHeroCardId(e);

            //    return;
            //}

        }

        //private int CountDeckCardNum(int cost,bool is_minion, bool is_spell,List<HSRangerLib.GameArenaDraftEventArgs.DeckCard> deck)
        //{
        //    int num = 0;

        //    foreach (var item in deck)
        //    {
        //        CardDef def = CardDefDB.Instance.GetCardDef(item.card_id);

        //        if (def.Cost == cost)
        //        {
        //            if (is_minion)
        //            {
        //                if (def.CardType == TAG_CARDTYPE.MINION)
        //                {
        //                    num += item.num;
        //                }
        //            }

        //            if (is_spell)
        //            {
        //                if (def.CardType == TAG_CARDTYPE.ABILITY ||
        //                    def.CardType == TAG_CARDTYPE.ENCHANTMENT)
        //                {
        //                    num += item.num;
        //                }
        //            }
        //        }
        //    }

        //    return num;
        //}

        //private int f(string hero_id)
        //{
        //    CardDef def = CardDefDB.Instance.GetCardDef(hero_id);

        //    //No.1 choice (Your best choice)
        //    if (def.Class == TAG_CLASS.DRUID)
        //    {
        //        return 1;
        //    }

        //    //No.2 choice
        //    if (def.Class == TAG_CLASS.HUNTER)
        //    {
        //        return 2;
        //    }

        //    //No.3 choice
        //    if (def.Class == TAG_CLASS.MAGE)
        //    {
        //        return 3;
        //    }

        //    //No.4 choice
        //    if (def.Class == TAG_CLASS.PALADIN)
        //    {
        //        return 4; 
        //    }

        //    //No.5 choice
        //    if (def.Class == TAG_CLASS.PRIEST)
        //    {
        //        return 5;
        //    }

        //    //No.6 choice
        //    if (def.Class == TAG_CLASS.ROGUE)
        //    {
        //        return 6;
        //    }
        //    //No.7 choice
        //    if (def.Class == TAG_CLASS.SHAMAN)
        //    {
        //        return 7;
        //    }
        //    //No.8 choice
        //    if (def.Class == TAG_CLASS.WARLOCK)
        //    {
        //        return 8;
        //    }
        //    //No.9 choice
        //    if (def.Class == TAG_CLASS.WARRIOR)
        //    {
        //        return 9;
        //    }

        //    return 100;
        //}

        //private string GetBestHeroCardId(GameArenaDraftEventArgs e)
        //{
        //    string best_hero_id = "";
        //    foreach (var card_id in e.draft_choices.OrderBy( hero => GetHeroPriority(hero)))
        //    {
        //        best_hero_id = card_id;
        //        break;
        //    }

        //    return best_hero_id;
        //}
    






        /// <summary>
        /// invoke when game starts.
        /// </summary>
        /// <param name="e">e.deck_list -- all cards id in the deck.</param>
        public override void OnGameStart(GameStartEventArgs e)
        {
            // reset instance vars
            numExecsReceived = 0;
            numActionsSent = 0;

            if (Hrtprozis.Instance.deckName != e.deck_name)
            {
                Helpfunctions.Instance.ErrorLog("New Deck: \"" + e.deck_name + "\", Old Deck: \"" + Hrtprozis.Instance.deckName + "\"");
                deckChanged = true;
                Hrtprozis.Instance.setDeckName(e.deck_name);
            }
            else
            {
                deckChanged = false;
            }

            Hrtprozis.Instance.clearDecks();
            foreach (var card in e.deck_list)
            {
                Hrtprozis.Instance.addCardToDecks(CardDB.Instance.cardIdstringToEnum(card.card_id), card.num);
            }

        }

        /// <summary>
        /// invoke when game ends.
        /// </summary>
        /// <param name="e"></param>
        public override void OnGameOver(GameOverEventArgs e)
        {
            if (e.win)
            {
                HandleWining();
            }else if (e.loss || e.concede)
            {
                HandleLosing(e.concede);
            }
        }

        private HSRangerLib.BotAction CreateRangerConcedeAction()
        {
            HSRangerLib.BotAction ranger_action = new HSRangerLib.BotAction();
            ranger_action.Actor = base.FriendHero;
            ranger_action.Type = BotActionType.CONCEDE;

            return ranger_action;
        }

        private HSRangerLib.BotActionType GetRangerActionType(Entity actor, Entity target, actionEnum sf_action_type)
        {
            
            if (sf_action_type == actionEnum.endturn)
            {
                if (POWERFULSINGLEACTION >= 1) POWERFULSINGLEACTION = 0;
                return BotActionType.END_TURN;
            }

            if (sf_action_type == actionEnum.useHeroPower)
            {
                return BotActionType.CAST_ABILITY;
            }

            if (sf_action_type == actionEnum.attackWithHero)
            {
                return BotActionType.HERO_ATTACK;
            }

            if (sf_action_type == actionEnum.attackWithMinion)
            {
                if (actor.Zone == HSRangerLib.TAG_ZONE.HAND && actor.IsMinion)
                {
                    return BotActionType.CAST_MINION;// that should not occour >_>
                }else if (actor.Zone == HSRangerLib.TAG_ZONE.PLAY && actor.IsMinion)
                {
                    return BotActionType.MINION_ATTACK;
                }
            }

            if (sf_action_type == actionEnum.playcard)
            {
                if (actor.Zone == HSRangerLib.TAG_ZONE.HAND)
                {
                    if (actor.IsMinion)
                    {
                        return BotActionType.CAST_MINION;
                    }else if (actor.IsWeapon)
                    {
                        return BotActionType.CAST_WEAPON;
                    }else
                    {
                        return BotActionType.CAST_SPELL;
                    }                    
                }else if (actor.Zone == HSRangerLib.TAG_ZONE.PLAY)
                {
                    if (actor.IsMinion)
                    {
                        return BotActionType.MINION_ATTACK;
                    }else if (actor.IsWeapon)
                    {
                        return BotActionType.HERO_ATTACK;
                    }
                }
            }

            if (target != null)
            {
                Helpfunctions.Instance.ErrorLog("GetActionType: wrong action type! " +
                                            sf_action_type.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(actor.CardId)
                                                         + " target: " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(target.CardId));
            }else
            {
                Helpfunctions.Instance.ErrorLog("GetActionType: wrong action type! " +
                                            sf_action_type.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(actor.CardId)
                                                         + " target none.");
            }

            if (POWERFULSINGLEACTION >= 1) POWERFULSINGLEACTION = 0;
            return BotActionType.END_TURN;
        }

        private HSRangerLib.BotAction ConvertToRangerAction(Action moveTodo)
        {
            HSRangerLib.BotAction ranger_action = new HSRangerLib.BotAction();
            Ai daum = Ai.Instance;



            switch (moveTodo.actionType)
            {
                case actionEnum.endturn:
                    if (POWERFULSINGLEACTION >= 1) POWERFULSINGLEACTION = 0;
                    break;
                case actionEnum.playcard:
                    ranger_action.Actor = getCardWithNumber(moveTodo.card.entity);

                    if (daum.bestmove.actionType == actionEnum.playcard && daum.bestmove != null)
                    {
                        if (daum.IsPlayRandomEffect(daum.bestmove.card.card, daum.oldMoveGuess, daum.nextMoveGuess))
                        {
                            this.doMultipleThingsAtATime = false;
                            this.dontmultiactioncount++;
                            //Helpfunctions.Instance.ErrorLog("doMultipleThingsAtATime " + doMultipleThingsAtATime + " because IsPlayRandomEffect 찾는거");

                        }
                        else this.doMultipleThingsAtATime = true;


                        if (daum.bestmove.card.card.name == CardDB.cardName.barnes) POWERFULSINGLEACTION++;

                        switch (daum.bestmove.card.card.name)
                        {
                            case CardDB.cardName.defenderofargus:
                            case CardDB.cardName.direwolfalpha:
                            case CardDB.cardName.darkpeddler:
                            case CardDB.cardName.quickshot:
                            case CardDB.cardName.kingselekk:
                            case CardDB.cardName.barnes:
                            case CardDB.cardName.tuskarrtotemic:
                            case CardDB.cardName.flametonguetotem:
                            case CardDB.cardName.leeroyjenkins:
                                //Helpfunctions.Instance.logg("찾는거 " + daum.bestmove.card.card.name + " 드로우카드 찾는거");
                                //Helpfunctions.Instance.ErrorLog("찾는거 " + daum.bestmove.card.card.name + " 드로우카드 찾는거");
                                //case CardDB.cardName.defenderofargus:
                                this.doMultipleThingsAtATime = false;
                                this.dontmultiactioncount++; break;
                            default: break;
                        }

                        if (PenalityManager.Instance.cardDrawBattleCryDatabase.ContainsKey(daum.bestmove.card.card.name))
                        {
                            this.doMultipleThingsAtATime = false;
                            this.dontmultiactioncount++; break;
                        }

                        //charge
                        if (daum.bestmove.card.card.Charge)
                        {
                            this.doMultipleThingsAtATime = false;
                            this.POWERFULSINGLEACTION++;
                            this.dontmultiactioncount++; break;

                        }
                        else
                        {
                            //special charge
                            switch (daum.bestmove.card.card.name)
                            {
                                case CardDB.cardName.leathercladhogleader:
                                    if (Playfield.Instance.EnemyCards.Count >= 6)
                                    {
                                        this.doMultipleThingsAtATime = false;
                                        this.POWERFULSINGLEACTION++;
                                        this.dontmultiactioncount++; break;
                                    }
                                    else break;
                                case CardDB.cardName.southseadeckhand:
                                    this.POWERFULSINGLEACTION++;
                                    break;
                                case CardDB.cardName.spikedhogrider:
                                    if (Playfield.Instance.enemyMinions.Find(a => a.taunt) != null)
                                    {
                                        this.doMultipleThingsAtATime = false;
                                        this.POWERFULSINGLEACTION++;
                                        this.dontmultiactioncount++; break;
                                    }
                                    else break;
                                case CardDB.cardName.alexstraszaschampion:
                                    if (Playfield.Instance.owncards.Find(a => a.card.race == TAG_RACE.DRAGON) != null)
                                    {
                                        this.doMultipleThingsAtATime = false;
                                        this.POWERFULSINGLEACTION++;
                                        this.dontmultiactioncount++; break;
                                    }
                                    else break;
                                case CardDB.cardName.tanarishogchopper:
                                    if (Playfield.Instance.EnemyCards.Count == 6)
                                    {
                                        this.doMultipleThingsAtATime = false;
                                        this.POWERFULSINGLEACTION++;
                                        this.dontmultiactioncount++; break;
                                    }
                                    else break;
                                case CardDB.cardName.armoredwarhorse:
                                    this.doMultipleThingsAtATime = false;
                                    this.POWERFULSINGLEACTION++;
                                    this.dontmultiactioncount++; break;
                                default: break;
                            }
                        }


                        bool hasjuggler = false;
                        foreach (Minion m in Playfield.Instance.ownMinions)
                        {
                            if (m.name == CardDB.cardName.knifejuggler && !m.silenced) hasjuggler = true;
                        }
                        if (hasjuggler && (daum.bestmove.card.card.type == CardDB.cardtype.MOB || PenalityManager.Instance.summonMinionSpellsDatabase.ContainsKey(daum.bestmove.card.card.name)))
                        {
                            bool hasdamageeffectminion = false;
                            foreach (Minion m in Playfield.Instance.enemyMinions)
                            {
                                if (m.name == CardDB.cardName.impgangboss ||
                                    m.name == CardDB.cardName.dragonegg ||
                                    m.name == CardDB.cardName.hoggerdoomofelwynn ||
                                    m.name == CardDB.cardName.grimpatron) hasdamageeffectminion = true;
                                if (!m.silenced && (m.handcard.card.deathrattle || m.hasDeathrattle())) hasdamageeffectminion = true;
                            }
                            if (hasdamageeffectminion) this.POWERFULSINGLEACTION++;
                            Helpfunctions.Instance.logg("찾는거 저글러 몹" + daum.bestmove.card.card.name);
                            Helpfunctions.Instance.logg("찾는거 저글러 몹" + daum.bestmove.card.card.name);
                            Helpfunctions.Instance.ErrorLog("찾는거 저글러 몹" + daum.bestmove.card.card.name);
                            Helpfunctions.Instance.ErrorLog("찾는거 저글러 몹" + daum.bestmove.card.card.name);
                        }

                        
                        


                        if (daum.bestmove.card.card.type == CardDB.cardtype.SPELL)
                        {

                            if (daum.bestmove.card.card.name == CardDB.cardName.jadeidol &&
                                (daum.bestmove.druidchoice == 2 || Playfield.Instance.ownMinions.Find(a => a.name == CardDB.cardName.fandralstaghelm && !a.silenced) != null))
                            {
                                Hrtprozis.Instance.AddTurnDeck(CardDB.cardIDEnum.CFM_602, 3);
                            }


                            this.dontmultiactioncount++;
                            bool hasenemydeathrattle = false;
                            foreach (Entity mnn in EnemyMinion)
                            {
                                if (mnn.HasDeathrattle) hasenemydeathrattle = true;
                            }

                            bool Random_Spell_But_Can_Kill_Deathrattle_Card = false;
                            switch (daum.bestmove.card.card.name)
                            {
                                case CardDB.cardName.arcanemissiles:
                                case CardDB.cardName.forkedlightning:
                                case CardDB.cardName.brawl:
                                case CardDB.cardName.bouncingblade:      
                                case CardDB.cardName.darkbargain:
                                case CardDB.cardName.avengingwrath:
                                case CardDB.cardName.multishot:
                                case CardDB.cardName.fistofjaraxxus:
                                case CardDB.cardName.deadlyshot: 
                                case CardDB.cardName.sabotage:
                                case CardDB.cardName.spreadingmadness:
                                case CardDB.cardName.flamecannon: 
                                case CardDB.cardName.cleave: Random_Spell_But_Can_Kill_Deathrattle_Card = true;
                                    this.POWERFULSINGLEACTION++;
                                    break;
                                default: break;
                            }

                            bool hastargetdeathrattle = false;
                            if (daum.bestmove.target != null) hastargetdeathrattle = (daum.bestmove.target.hasDeathrattle() || daum.bestmove.target.deathrattles.Count >= 1 || (daum.bestmove.target.handcard.card.deathrattle && !daum.bestmove.target.silenced)) && !daum.bestmove.target.isHero;

                            bool targethasdamageeffect = false;
                            if (daum.bestmove.target != null)
                            {
                                switch (daum.bestmove.target.name)
                                {
                                    case CardDB.cardName.impgangboss:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.dragonegg:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.hoggerdoomofelwynn:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.grimpatron:
                                        targethasdamageeffect = true; break;
                                    default:
                                        break;
                                }
                            }
                            bool hasdamageeffectminion = false;
                            foreach (Minion m in Playfield.Instance.enemyMinions)
                            {
                                if (m.name == CardDB.cardName.impgangboss ||
                                    m.name == CardDB.cardName.dragonegg ||
                                    m.name == CardDB.cardName.hoggerdoomofelwynn ||
                                    m.name == CardDB.cardName.grimpatron) hasdamageeffectminion = true;
                            }                            
                            if ((daum.bestmove.target != null && (hastargetdeathrattle || targethasdamageeffect) || 
                                ((hasenemydeathrattle || hasdamageeffectminion) && (Random_Spell_But_Can_Kill_Deathrattle_Card || PenalityManager.Instance.DamageAllDatabase.ContainsKey(daum.bestmove.card.card.name) || PenalityManager.Instance.DamageAllEnemysDatabase.ContainsKey(daum.bestmove.card.card.name)))))
                            {
                                this.POWERFULSINGLEACTION++;

                            }

                            //switch (daum.bestmove.card.card.name)
                            //{
                            //    case CardDB.cardName.hex:
                            //    case CardDB.cardName.jadeidol:
                            //        this.POWERFULSINGLEACTION++; break;
                            //    default: break;
                            //}                                           
                        }

                        foreach (Minion m in Playfield.Instance.ownMinions)
                        {
                            if (m.name == CardDB.cardName.fandralstaghelm && !m.silenced)
                            {
                                this.doMultipleThingsAtATime = false;
                                this.dontmultiactioncount++;
                                this.POWERFULSINGLEACTION++;
                            }
                        }

                    }

                    //if (moveTodo.card.card.type == CardDB.cardtype.MOB || moveTodo.card.card.name == CardDB.cardName.forbiddenritual)
                    //{
                    //    foreach (Minion mnn in Playfield.Instance.ownMinions)
                    //    {
                    //        if (!mnn.silenced && (mnn.name == CardDB.cardName.darkshirecouncilman || mnn.name == CardDB.cardName.knifejuggler))
                    //        {
                    //            System.Threading.Thread.Sleep(500);
                    //            Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler 미리 effect detected sleep 500ms");
                    //            Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler 미리 effect detected sleep 500ms");
                    //            Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler 미리 effect detected sleep 500ms");
                    //            Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler 미리 effect detected sleep 500ms");
                    //        }
                    //    }
                    //}

                    if (moveTodo.card.card.type == CardDB.cardtype.MOB)
                    {
                        System.Threading.Thread.Sleep(200);
                    }

                    switch (moveTodo.card.card.name)
                    {
                        case CardDB.cardName.defenderofargus:
                            if (this.EnemySecrets.Count >= 1) System.Threading.Thread.Sleep(1800);
                            System.Threading.Thread.Sleep(4500); 
                            break;
                        case CardDB.cardName.abusivesergeant:
                            System.Threading.Thread.Sleep(900); break;
                        case CardDB.cardName.darkirondwarf:
                            System.Threading.Thread.Sleep(900); break;
                        case CardDB.cardName.direwolfalpha:
                            if (this.EnemySecrets.Count >= 1) System.Threading.Thread.Sleep(1200);
                            System.Threading.Thread.Sleep(2500);
                            break;
                        case CardDB.cardName.flametonguetotem:
                            if (this.EnemySecrets.Count >= 1) System.Threading.Thread.Sleep(1200);
                            System.Threading.Thread.Sleep(2500);
                            break;
                        default:
                            break;
                    }

                    if (ranger_action.Actor == null) return null;  // missing entity likely because new spawned minion
                    break;
                case actionEnum.attackWithHero:
                    ranger_action.Actor = base.FriendHero;
                    System.Threading.Thread.Sleep(1100);

                    foreach (Minion m in Playfield.Instance.ownMinions)
                    {
                        if (m.name == CardDB.cardName.southseadeckhand && (m.playedThisTurn || m.Ready ))
                        {
                            this.doMultipleThingsAtATime = false;
                            this.dontmultiactioncount++;
                            this.POWERFULSINGLEACTION++;
                        }
                    }

                    break;
                case actionEnum.useHeroPower:
                    ranger_action.Actor = base.FriendHeroPower;
                    break;
                case actionEnum.attackWithMinion:
                    ranger_action.Actor = getEntityWithNumber(moveTodo.own.entityID);

                    //foreach (Minion m in Playfield.Instance.ownMinions)
                    //{
                    //    if (m.name == CardDB.cardName.flametonguetotem || m.name == CardDB.cardName.direwolfalpha || (m.name == CardDB.cardName.frothingberserker && m.Ready && !m.frozen)
                    //        || m.name == CardDB.cardName.southseadeckhand && m.Ready)
                    //    {
                    //        this.doMultipleThingsAtATime = false;
                    //        this.dontmultiactioncount++;
                    //        this.POWERFULSINGLEACTION++;
                    //    }
                    //}

                    if (ranger_action.Actor == null) return null;  // missing entity likely because new spawned minion
                    break;
                default:
                    break;
            }

            if (moveTodo.target != null)
            {
                ranger_action.Target = getEntityWithNumber(moveTodo.target.entityID);
                if (ranger_action.Target == null) return null;  // missing entity likely because new spawned minion
            }


             ranger_action.Type = GetRangerActionType(ranger_action.Actor, ranger_action.Target, moveTodo.actionType);

             if (moveTodo.druidchoice >= 1)
             {
                 ranger_action.Choice = moveTodo.druidchoice;//1=leftcard, 2= rightcard
             }

             ranger_action.Index = moveTodo.place;
             if (moveTodo.place >= 1) ranger_action.Index = moveTodo.place - 1;

             if (moveTodo.target != null)
             {
                 //ranger stuff :D
                 ranger_action.ID = moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId);

                 Helpfunctions.Instance.ErrorLog(moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId)
                                                  + " target: " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Target.CardId));
                 Helpfunctions.Instance.logg(moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId)
                                                  + " target: " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Target.CardId)
                                                  + " choice: " + moveTodo.druidchoice + " place" + moveTodo.place);


             }
             else
             {
                 //ranger stuff :D
                 ranger_action.ID = moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId);

                 Helpfunctions.Instance.ErrorLog(moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId)
                                                  + " target nothing");
                 Helpfunctions.Instance.logg(moveTodo.actionType.ToString() + ": " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Actor.CardId)
                                                  + " choice: " + moveTodo.druidchoice + " place" + moveTodo.place);
             }





            if (moveTodo.actionType == actionEnum.attackWithMinion)
            {

                if (moveTodo.target != null && 
                    (!ranger_action.Target.IsHero && 
                    (ranger_action.Target.HasDeathrattle && !ranger_action.Target.IsSilenced) ||
                    (ranger_action.Actor.HasDeathrattle && !ranger_action.Actor.IsSilenced)
                    ))
                {
                    this.doMultipleThingsAtATime = false;

                    this.dontmultiactioncount++;

                    //this.POWERFULSINGLEACTION++;

                    //Helpfunctions.Instance.ErrorLog("doMultipleThingsAtATime attackWithMinion " + doMultipleThingsAtATime + " because " + HSRangerLib.CardDefDB.Instance.GetCardEnglishName(ranger_action.Target.CardId));
                }

                else if (POWERFULSINGLEACTION >= 1)
                {
                    this.doMultipleThingsAtATime = false;
                }
                else this.doMultipleThingsAtATime = true;
            }


            if (this.EnemySecrets.Count >= 1)
            {
                this.doMultipleThingsAtATime = false;
                //this.POWERFULSINGLEACTION++;
                //Helpfunctions.Instance.ErrorLog("찾는거 doMultipleThingsAtATime " + doMultipleThingsAtATime + " because enemy secret");
                //Helpfunctions.Instance.logg("찾는거 doMultipleThingsAtATime " + doMultipleThingsAtATime + " because enemy secret");
            }
            

            if (moveTodo.actionType == actionEnum.attackWithMinion && ranger_action.Target.IsHero && this.EnemyMinion.Count == 0)
            {
                this.doMultipleThingsAtATime = true;
                this.dontmultiactioncount = 0;
            }
            if (POWERFULSINGLEACTION >= 1) 
            {
                this.doMultipleThingsAtATime = false;
                dontmultiactioncount++;
            }

            return ranger_action;
        }

        /// <summary>
        /// if uses extern a.i.,
        /// invoke when hearthranger did all the actions.
        /// </summary>
        /// <param name="e"></param>
        public override void OnQueryBestMove(QueryBestMoveEventArgs e)
        {

            //don't forget to set HasBestMoveAI property to true in class constructor.
            //or Hearthranger will never query best move !
            //base.HasBestMoveAI = true;
            e.handled = true;
            HSRangerLib.BotAction ranger_action;

            try
            {
                Helpfunctions.Instance.ErrorLog("start things...");
                //HR-only fix for being too fast
                //IsProcessingPowers not good enough so always sleep
                //System.Threading.Thread.Sleep(200);
                //todo find better solution
                //better test... we checked if isprocessing is true.. after that, we wait little time and test it again.
                if (this.gameState.IsProcessingPowers || this.gameState.IsBlockingServer || this.gameState.IsBusy || this.gameState.IsMulliganBlockingPowers)
                {
                    Helpfunctions.Instance.logg("HR is too fast...");
                    Helpfunctions.Instance.ErrorLog("HR is too fast...");
                    if (this.gameState.IsProcessingPowers) Helpfunctions.Instance.logg("IsProcessingPowers");
                    if (this.gameState.IsBlockingServer) Helpfunctions.Instance.logg("IsBlockingServer");
                    if (this.gameState.IsBusy) Helpfunctions.Instance.logg("IsBusy");
                    if (this.gameState.IsMulliganBlockingPowers) Helpfunctions.Instance.logg("IsMulliganBlockingPowers");
                }

                Helpfunctions.Instance.ErrorLog("proc check done...");


                //we are conceding
                if (this.isgoingtoconcede)
                {
                    if (HSRangerLib.RangerBotSettings.CurrentSettingsGameType == HSRangerLib.enGameType.The_Arena)
                    {
                        this.isgoingtoconcede = false;
                    }
                    else
                    {
                        ranger_action = CreateRangerConcedeAction();
                        e.action_list.Add(ranger_action);
                        return;
                    }
                }
                if (Settings.Instance.learnmode)
                {
                    e.handled = false;
                    return;
                }

                Helpfunctions.Instance.ErrorLog("update everything...");
                bool templearn = sf.updateEverything(this, behave, doMultipleThingsAtATime, Settings.Instance.useExternalProcess, false); // cant use passive waiting (in this mode i return nothing)
                if (templearn == true) Settings.Instance.printlearnmode = true;

                // actions-queue-stuff
                //  AI has requested to ignore this update, so return without setting any actions.
                if (!shouldSendActions)
                {
                    //Helpfunctions.Instance.ErrorLog("shouldsendactionsblah");
                    shouldSendActions = true;  // unpause ourselves for next time
                    return;
                }


                if (Settings.Instance.learnmode)
                {
                    if (Settings.Instance.printlearnmode)
                    {
                        Ai.Instance.simmulateWholeTurnandPrint();
                    }
                    Settings.Instance.printlearnmode = false;

                    e.handled = false;
                    return;
                }

                if (Settings.Instance.enemyConcede) Helpfunctions.Instance.ErrorLog("bestmoveVal:" + Ai.Instance.bestmoveValue);
                
                if (Ai.Instance.bestmoveValue <= Settings.Instance.enemyConcedeValue && Settings.Instance.enemyConcede)
                {
                    e.action_list.Add(CreateRangerConcedeAction());
                    return;
                }

                if (Handmanager.Instance.getNumberChoices() >= 1)
                {
                    //detect which choice

                    int trackingchoice = Ai.Instance.bestTracking;
                    if (Ai.Instance.bestTrackingStatus == 3) Helpfunctions.Instance.logg("discovering using user choice..." + trackingchoice);
                    if (Ai.Instance.bestTrackingStatus == 0) Helpfunctions.Instance.logg("discovering using optimal choice..." + trackingchoice);
                    if (Ai.Instance.bestTrackingStatus == 1) Helpfunctions.Instance.logg("discovering using suboptimal choice..." + trackingchoice);
                    if (Ai.Instance.bestTrackingStatus == 2) Helpfunctions.Instance.logg("discovering using random choice..." + trackingchoice);

                    trackingchoice = Silverfish.Instance.choiceCardsEntitys[trackingchoice - 1];
                    
                    //there is a tracking/discover effect ongoing! (not druid choice)
                    BotAction trackingaction = new HSRangerLib.BotAction();
                    trackingaction.Actor = this.getEntityWithNumber(trackingchoice);
                    Helpfunctions.Instance.logg("discovering choice entity" + trackingchoice + " card " + trackingaction.Actor.CardId);

                    //DEBUG stuff
                    Helpfunctions.Instance.logg("actor: cardid " + trackingaction.Actor.CardId + " entity " + trackingaction.Actor.EntityId);
                    
                    e.action_list.Add(trackingaction);
                    
                    //string filename = "silvererror" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".xml";
                    //Helpfunctions.Instance.logg("create errorfile " +  filename);
                    //this.gameState.SaveToXMLFile(filename);
                    return;
                }

                if (!doMultipleThingsAtATime || this.dontmultiactioncount >= 1)
                {
                    //this is used if you cant queue actions (so ai is just sending one action at a time)
                    Action moveTodo = Ai.Instance.bestmove;

                    //Helpfunctions.Instance.ErrorLog("dontmultiactioncount " + dontmultiactioncount);

                    if (moveTodo == null || moveTodo.actionType == actionEnum.endturn)
                    {
                        //simply clear action list, hearthranger bot will endturn if no action can do.
                        e.action_list.Clear();
                        BotAction endturnmove = new HSRangerLib.BotAction();
                        endturnmove.Type = BotActionType.END_TURN;
                        Helpfunctions.Instance.ErrorLog("end turn action");
                        e.action_list.Add(endturnmove);
                        if (POWERFULSINGLEACTION >= 1 || dontmultiactioncount >= 1)
                        {
                            Helpfunctions.Instance.ErrorLog("찾는거종료1" + POWERFULSINGLEACTION);
                            Helpfunctions.Instance.logg("찾는거종료1" + POWERFULSINGLEACTION);
                            Helpfunctions.Instance.ErrorLog("찾는거종료1" + dontmultiactioncount);
                            Helpfunctions.Instance.logg("찾는거종료1" + dontmultiactioncount);
                            Helpfunctions.Instance.ErrorLog("찾는거종료1 doMultipleThingsAtATime " + doMultipleThingsAtATime);
                            Helpfunctions.Instance.logg("찾는거종료1 doMultipleThingsAtATime " + doMultipleThingsAtATime);
                            POWERFULSINGLEACTION = 0;
                            dontmultiactioncount = 0;
                            doMultipleThingsAtATime = true;
                        }
                        doMultipleThingsAtATime = true;
                        return;
                    }
                    else
                    {
                        shouldSendFakeAction = true;
                    }

                    Helpfunctions.Instance.ErrorLog("play action");
                    moveTodo.print();

                    e.action_list.Add(ConvertToRangerAction(moveTodo));

                    this.dontmultiactioncount = 0;
                }
                else
                {//##########################################################################
                    //this is used if you can queue multiple actions
                    //thanks to xytrix

                    this.queuedMoveGuesses.Clear();
                    this.queuedMoveGuesses.Add(new Playfield());  // prior to any changes, in case HR fails to execute any actions
                    bool hasMoreActions = false;

                    do
                    {
                        Helpfunctions.Instance.ErrorLog("play action..." + (e.action_list.Count() + 1));
                        Action moveTodo = Ai.Instance.bestmove;

                        if (!hasMoreActions && (moveTodo == null || moveTodo.actionType == actionEnum.endturn))
                        {
                            Helpfunctions.Instance.ErrorLog("enturn");
                            //simply clear action list, hearthranger bot will endturn if no action can do.
                            BotAction endturnmove = new HSRangerLib.BotAction();
                            endturnmove.Type = BotActionType.END_TURN;
                            e.action_list.Add(endturnmove);
                            hasMoreActions = false;
                        }
                        else
                        {

                            Helpfunctions.Instance.ErrorLog("play action");
                            moveTodo.print();

                            BotAction nextMove = ConvertToRangerAction(moveTodo);
                            if (nextMove == null) return;  // Prevent exceptions for expected errors like missing entityID for new spawned minions

                            e.action_list.Add(nextMove);
                            this.queuedMoveGuesses.Add(new Playfield(Ai.Instance.nextMoveGuess));

                            hasMoreActions = canQueueNextActions();
                            if (hasMoreActions) Ai.Instance.doNextCalcedMove();
                        }
                    }
                    while (hasMoreActions);

                    numActionsSent = e.action_list.Count();
                    Helpfunctions.Instance.ErrorLog("sending HR " + numActionsSent + " queued actions");
                    numExecsReceived = 0;
                }//##########################################################################


            }
            catch (Exception Exception)
            {
                using (StreamWriter sw = File.AppendText(Settings.Instance.logpath + "CrashLog" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".txt"))
                {
                    sw.WriteLine(Exception.ToString());
                }
                Helpfunctions.Instance.logg("\r\nDLL Crashed! " + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + "\r\nStackTrace ---" + Exception.ToString() + "\r\n\r\n");
                Helpfunctions.Instance.flushLogg();
                Helpfunctions.Instance.flushErrorLog();

                if (Settings.Instance.learnmode)
                {
                    e.action_list.Clear();
                }
                throw;
            }
            return;
        }


        public override void OnActionDone(ActionDoneEventArgs e)
        {
            //do nothing here

            //queue stuff
            numExecsReceived++;

            switch (e.done_result)
            {
                case ActionDoneEventArgs.ActionResult.Executed:
                    Helpfunctions.Instance.ErrorLog("HR action " + numExecsReceived + " done <executed>: " + e.action_id); break;
                case ActionDoneEventArgs.ActionResult.SourceInvalid:
                    Helpfunctions.Instance.ErrorLog("HR action " + numExecsReceived + " done <invalid_source>: " + e.action_id); break;
                case ActionDoneEventArgs.ActionResult.TargetInvalid:
                    Helpfunctions.Instance.ErrorLog("HR action " + numExecsReceived + " done <invalid_target>: " + e.action_id); break;
                default:
                    Helpfunctions.Instance.ErrorLog("HR action " + numExecsReceived + " done <default>: " + e.action_id + " " + e.ToString()); break;
            }

        }


        private bool canQueueNextActions()
        {
            if (!Ai.Instance.canQueueNextMoves()) return false;

            // HearthRanger will re-query bestmove after a targeted minion buff. So even though we can queue moves after,
            // there's no point because we'll just print error messages when HearthRanger ignores them.
            if (Ai.Instance.bestmove.actionType == actionEnum.playcard)
            {
                CardDB.cardName card = Ai.Instance.bestmove.card.card.name;

                if (card == CardDB.cardName.abusivesergeant
                    || card == CardDB.cardName.darkirondwarf
                    || card == CardDB.cardName.crueltaskmaster
                    || card == CardDB.cardName.screwjankclunker
                    || card == CardDB.cardName.lancecarrier
                    || card == CardDB.cardName.clockworkknight
                    || card == CardDB.cardName.shatteredsuncleric
                    || card == CardDB.cardName.houndmaster
                    || card == CardDB.cardName.templeenforcer
                    || card == CardDB.cardName.wildwalker
                    || card == CardDB.cardName.defenderofargus
                    || card == CardDB.cardName.direwolfalpha
                    || card == CardDB.cardName.flametonguetotem
                    || card == CardDB.cardName.darkpeddler
                    || card == CardDB.cardName.kingselekk
                    || card == CardDB.cardName.animalcompanion
                    || card == CardDB.cardName.barnes
                    || card == CardDB.cardName.tuskarrtotemic
                    || card == CardDB.cardName.callofthewild
                    || card == CardDB.cardName.quickshot
                    || card == CardDB.cardName.unleashthehounds
                    || card == CardDB.cardName.southseadeckhand)
                {
                    return false;
                }

                if (Ai.Instance.bestmove.card.card.type == CardDB.cardtype.MOB || PenalityManager.Instance.summonMinionSpellsDatabase.ContainsKey(Ai.Instance.bestmove.card.card.name))
                {
                    foreach (Minion mnn in Playfield.Instance.ownMinions)
                    {
                        if (!mnn.silenced && (mnn.name == CardDB.cardName.darkshirecouncilman || mnn.name == CardDB.cardName.knifejuggler))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        int lossedtodo = 0;
        int KeepConcede = 0;
        int oldwin = 0;
        private bool autoconcede()
        {
            if (HSRangerLib.RangerBotSettings.CurrentSettingsGameType == HSRangerLib.enGameType.The_Arena) return false;
            if (HSRangerLib.RangerBotSettings.CurrentSettingsGameType == HSRangerLib.enGameType.Play_Ranked) return false;
            int totalwin = this.wins;
            int totallose = this.loses;
            /*if ((totalwin + totallose - KeepConcede) != 0)
            {
                Helpfunctions.Instance.ErrorLog("#info: win:" + totalwin + " concede:" + KeepConcede + " lose:" + (totallose - KeepConcede) + " real winrate:" + (totalwin * 100 / (totalwin + totallose - KeepConcede)));
            }*/



            int curlvl = gameState.CurrentRank;

            if (curlvl > this.concedeLvl)
            {
                this.lossedtodo = 0;
                return false;
            }

            if (this.oldwin != totalwin)
            {
                this.oldwin = totalwin;
                if (this.lossedtodo > 0)
                {
                    this.lossedtodo--;
                }
                Helpfunctions.Instance.ErrorLog("not today!! (you won a game)");
                this.isgoingtoconcede = true;
                return true;
            }

            if (this.lossedtodo > 0)
            {
                this.lossedtodo--;
                Helpfunctions.Instance.ErrorLog("not today!");
                this.isgoingtoconcede = true;
                return true;
            }

            if (curlvl < this.concedeLvl)
            {
                this.lossedtodo = 3;
                Helpfunctions.Instance.ErrorLog("your rank is " + curlvl + " targeted rank is " + this.concedeLvl + " -> concede!");
                Helpfunctions.Instance.ErrorLog("not today!!!");
                this.isgoingtoconcede = true;
                return true;
            }
            return false;
        }

        private bool concedeVSenemy(string ownh, string enemyh)
        {
            if (HSRangerLib.RangerBotSettings.CurrentSettingsGameType == HSRangerLib.enGameType.The_Arena) return false;
            if (HSRangerLib.RangerBotSettings.CurrentSettingsGameType == HSRangerLib.enGameType.Play_Ranked) return false;

            if (Mulligan.Instance.shouldConcede(Hrtprozis.Instance.heroNametoEnum(ownh), Hrtprozis.Instance.heroNametoEnum(enemyh)))
            {
                Helpfunctions.Instance.ErrorLog("not today!!!!");
                this.isgoingtoconcede = true;
                return true;
            }
            return false;
        }

        private void HandleWining()
        {
            this.wins++;
            if (this.isgoingtoconcede)
            {
                this.isgoingtoconcede = false;
            }
            int totalwin = this.wins;
            int totallose = this.loses;
            if ((totalwin + totallose - KeepConcede) != 0)
            {
                Helpfunctions.Instance.ErrorLog("#info: win:" + totalwin + " concede:" + KeepConcede + " lose:" + (totallose - KeepConcede) + " real winrate:" + (totalwin * 100 / (totalwin + totallose - KeepConcede)));
            }
            else
            {
                Helpfunctions.Instance.ErrorLog("#info: win:" + totalwin + " concede:" + KeepConcede + " lose:" + (totallose - KeepConcede) + " real winrate: 100");
            }
            Helpfunctions.Instance.logg("Match Won!");
        }

        private void HandleLosing(bool is_concede)
        {
            this.loses++;
            if (is_concede)
            {
                this.isgoingtoconcede = false;
                this.KeepConcede++;
            }
            this.isgoingtoconcede = false;
            int totalwin = this.wins;
            int totallose = this.loses;
            if ((totalwin + totallose - KeepConcede) != 0)
            {
                Helpfunctions.Instance.ErrorLog("#info: win:" + totalwin + " concede:" + KeepConcede + " lose:" + (totallose - KeepConcede) + " real winrate:" + (totalwin * 100 / (totalwin + totallose - KeepConcede)));
            }
            else
            {
                Helpfunctions.Instance.ErrorLog("#info: win:" + totalwin + " concede:" + KeepConcede + " lose:" + (totallose - KeepConcede) + " real winrate: 100");
            }
            Helpfunctions.Instance.logg("Match Lost :(");

        }

        private Entity getEntityWithNumber(int number)
        {
            foreach (Entity e in gameState.GameEntityList)
            {
                if (number == e.EntityId) return e;
            }
            return null;
        }

        private Entity getCardWithNumber(int number)
        {
            foreach (Entity e in base.FriendHand)
            {
                if (number == e.EntityId) return e;
            }
            return null;
        }
    }

    public sealed class Silverfish
    {
        public string versionnumber = "130.0SE + redfish";
        private bool singleLog = false;
        private string botbehave = "rush";
        public bool waitingForSilver = false;

        public bool startedexe = false;

        Playfield lastpf;
        Settings sttngs = Settings.Instance;

        public List<Minion> ownMinions = new List<Minion>();
        public List<Minion> enemyMinions = new List<Minion>();
        List<Handmanager.Handcard> handCards = new List<Handmanager.Handcard>();
        int ownPlayerController = 0;
        List<string> ownSecretList = new List<string>();
        int enemySecretCount = 0;
        List<int> enemySecretList = new List<int>();

        int currentMana = 0;
        int ownMaxMana = 0;
        int numOptionPlayedThisTurn = 0;
        int numMinionsPlayedThisTurn = 0;
        int cardsPlayedThisTurn = 0;
        int ownOverload = 0;

        int enemyMaxMana = 0;

        string ownHeroWeapon = "";
        int heroWeaponAttack = 0;
        int heroWeaponDurability = 0;

        string enemyHeroWeapon = "";
        int enemyWeaponAttack = 0;
        int enemyWeaponDurability = 0;

        string heroname = "";
        string enemyHeroname = "";

        CardDB.Card heroAbility = new CardDB.Card();
        bool ownAbilityisReady = false;
        CardDB.Card enemyAbility = new CardDB.Card();

        int anzcards = 0;
        int enemyAnzCards = 0;

        int ownHeroFatigue = 0;
        int enemyHeroFatigue = 0;
        int ownDecksize = 0;
        int enemyDecksize = 0;

        Minion ownHero;
        Minion enemyHero;

        private int anzOgOwnCThunHpBonus = 0;
        private int anzOgOwnCThunAngrBonus = 0;
        private int anzOgOwnCThunTaunt = 0;

        private int OwnCrystalCore = 0;
        private int EnemyCrystalCore = 0;
        private bool ownMinionsCost0 = false;

        // NEW VALUES--

        int numberMinionsDiedThisTurn = 0;//todo need that value
        int ownCurrentOverload = 0;//todo get them! = number of overloaded Manacrystals for CURRENT turn (NOT RECALL_OWED !)
        int enemyOverload = 0;//todo need that value maybe
        int ownDragonConsort = 0;
        int enemyDragonConsort = 0;
        int ownLoathebs = 0;// number of loathebs WE PLAYED (so enemy has the buff)
        int enemyLoathebs = 0;
        int ownMillhouse = 0; // number of millhouse-manastorm WE PLAYED (so enemy has the buff)
        int enemyMillhouse = 0;
        int ownKirintor = 0;
        int ownPrepa = 0;

        // NEW VALUES#TGT#############################################################################################################
        // NEW VALUES#################################################################################################################
        int heroPowerUsesThisTurn = 0;
        int ownHeroPowerUsesThisGame = 0;
        int enemyHeroPowerUsesThisGame = 0;
        int lockandload = 0;
        int Stampede = 0;
        int ownsabo=0;//number of saboteurplays  of our player (so enemy has the buff)
        int enemysabo = 0;//number of saboteurplays  of enemy player (so we have the buff)
        int ownFenciCoaches = 0; // number of Fencing Coach-debuffs on our player 

        int enemyCursedCardsInHand = 0;

        //LOE stuff###############################################################################################################
        List<CardDB.cardIDEnum> choiceCards = new List<CardDB.cardIDEnum>(); // here we save all available tracking/discover cards ordered from left to right
        public List<int> choiceCardsEntitys = new List<int>(); //list of entitys same order as choiceCards
        
        private static HSRangerLib.GameState latestGameState;
        
        private static Silverfish instance;

        public static Silverfish Instance
        {
            get
            {
                return instance ?? (instance = new Silverfish());
            }
        }

        private Silverfish()
        {
            this.singleLog = Settings.Instance.writeToSingleFile;
            string path = SilverFishBotPath.AssemblyDirectory + "SilverLogs" + System.IO.Path.DirectorySeparatorChar;
            Directory.CreateDirectory(path);
            sttngs.setFilePath(SilverFishBotPath.AssemblyDirectory);

            if (singleLog)
            {
                sttngs.setLoggPath(SilverFishBotPath.LogPath + System.IO.Path.DirectorySeparatorChar);
                sttngs.setLoggFile("SilverLog.txt");
                Helpfunctions.Instance.createNewLoggfile();
            }
            else
            {
                sttngs.setLoggPath(path);
            }
            
            Helpfunctions.Instance.ErrorLog("init Silverfish");
            Helpfunctions.Instance.ErrorLog("setlogpath to:" + path);

            PenalityManager.Instance.setCombos();
            Mulligan m = Mulligan.Instance; // read the mulligan list
            Discovery d = Discovery.Instance; // read the discover list
            Settings.Instance.setSettings();
            if (Settings.Instance.useNetwork) FishNet.Instance.startClient();
            Helpfunctions.Instance.startFlushingLogBuffers();
        }

        public void setnewLoggFile()
        {
            Questmanager.Instance.Reset();
            OwnCrystalCore = 0;
            EnemyCrystalCore = 0;
            ownMinionsCost0 = false;

            Helpfunctions.Instance.flushLogg(); // flush the buffer before creating a new log
            if (!singleLog)
            {
                sttngs.setLoggFile("SilverLog" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".txt");
                Helpfunctions.Instance.createNewLoggfile();
                Helpfunctions.Instance.ErrorLog("#######################################################");
                Helpfunctions.Instance.ErrorLog("fight is logged in: " + sttngs.logpath + sttngs.logfile);
                Helpfunctions.Instance.ErrorLog("#######################################################");
            }
            else
            {
                sttngs.setLoggFile("UILogg.txt");
            }
        }

        public void setNewGame()
        {
            //todo sepefeets - move stuff here to make things more consistant between HR/HB versions
        }

        public bool updateEverything(HSRangerLib.BotBase rangerbot, Behavior botbase, bool queueActions, bool runExtern = false, bool passiveWait = false)
        {
            // data sync workaround for temp buffs - something is wrong
            /*if (lastpf != null && lastpf.playactions[0].actionType == actionEnum.playcard && (lastpf.playactions[0].card.card.name == CardDB.cardName.savageroar ||
                lastpf.playactions[0].card.card.name == CardDB.cardName.bloodlust || PenalityManager.Instance.buffing1TurnDatabase.ContainsKey(lastpf.playactions[0].card.card.name)))
            {
                System.Threading.Thread.Sleep(1500);
            }*/

            //string ename = "" + Ai.Instance.bestmove.card.card.name;


            Ai daum = Ai.Instance;

            if (daum.bestmove != null)// || daum.bestmove.actionType != actionEnum.endturn || daum.bestmove.actionType != actionEnum.useHeroPower)
            //if (daum.bestActions[0].actionType != actionEnum.endturn  && daum.bestActions.Count >= 1 && Playfield.Instance.turnCounter == 0)
            {
                switch (daum.bestmove.actionType)
                {
                    case actionEnum.endturn:
                        //Helpfunctions.Instance.logg("엔드턴 확인  ");
                        break;
                    case actionEnum.attackWithHero:
                        {
                            if (rangerbot.gameState.TimerState != TurnTimerState.COUNTDOWN)
                            {
                                bool targethasdamageeffect = false;
                                switch (daum.bestmove.target.name)
                                {
                                    case CardDB.cardName.impgangboss:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.dragonegg:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.hoggerdoomofelwynn:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.grimpatron:
                                        targethasdamageeffect = true; break;
                                    default:
                                        break;
                                }

                                if (targethasdamageeffect && !daum.bestmove.target.isHero)
                                {
                                    System.Threading.Thread.Sleep(3500);
                                }

                                if (daum.bestmove.target.hasDeathrattle() && daum.bestmove.own.Angr >= daum.bestmove.target.Hp && !daum.bestmove.target.isHero)
                                {
                                    System.Threading.Thread.Sleep(2200);
                                }

                                if (daum.bestmove.target.taunt && daum.bestmove.own.Angr >= daum.bestmove.target.Hp && !daum.bestmove.target.isHero)
                                {
                                    System.Threading.Thread.Sleep(800);
                                    //Helpfunctions.Instance.logg("Target Taunt detected sleep 800ms");
                                    //Helpfunctions.Instance.logg("Target Taunt detected sleep 800ms");
                                    //Helpfunctions.Instance.ErrorLog("Target Taunt detected sleep 800ms");
                                    //Helpfunctions.Instance.ErrorLog("Target Taunt detected sleep 800ms");
                                }

                                //if (this.enemySecretCount >= 1)
                                //{
                                //    int time = 6000 / this.enemySecretCount;
                                //    foreach (SecretItem si in Probabilitymaker.Instance.enemySecrets)
                                //    {
                                //        if (si.canBe_noblesacrifice)
                                //        {
                                //            System.Threading.Thread.Sleep(time);
                                //            Helpfunctions.Instance.logg("찾는거 덫발견 덫8 time: " + time);
                                //            Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫8 time: " + time);
                                //        }
                                //        else if (daum.bestmove.target.isHero)
                                //        {
                                //            if (si.canBe_explosive
                                //                //|| si.canBe_icebarrier
                                //                || si.canBe_beartrap)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫9 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫9 time: " + time);
                                //            }
                                //        }
                                //        else if (!daum.bestmove.target.isHero)
                                //        {
                                //            if (si.canBe_snaketrap)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫10 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫10 time: " + time);
                                //            }
                                //            else if (daum.bestmove.own.Angr >= daum.bestmove.target.Hp)
                                //            {
                                //                if (si.canBe_iceblock)
                                //                {
                                //                    System.Threading.Thread.Sleep(time);
                                //                    Helpfunctions.Instance.logg("찾는거 덫발견 덫13 time: " + time);
                                //                    Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫13 time: " + time);
                                //                }
                                //            }

                                //        }
                                //        else if (daum.bestmove.own.Angr >= daum.bestmove.target.Hp && !daum.bestmove.target.isHero)
                                //        {
                                //            if (si.canBe_effigy
                                //                || si.canBe_redemption
                                //                || si.canBe_avenge
                                //                || si.canBe_duplicate)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫11 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫11 time: " + time);
                                //            }
                                //        }
                                //    }

                                    
                                //}

                            



                            }
                            break;
                        }
                    case actionEnum.attackWithMinion:
                        {
                            //System.Threading.Thread.Sleep(55);
                            //Helpfunctions.Instance.logg("미니언공격 확인  " + daum.bestmove.own.name + " 으로 공격 " + daum.bestmove.target.name);
                            //Helpfunctions.Instance.ErrorLog("미니언공격 확인  " + daum.bestmove.own.name + " 으로 공격 " + daum.bestmove.target.name);

                            if (rangerbot.gameState.TimerState != TurnTimerState.COUNTDOWN)
                            {
                                bool targethasdamageeffect = false;
                                switch (daum.bestmove.target.name)
                                {
                                    case CardDB.cardName.impgangboss:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.dragonegg:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.hoggerdoomofelwynn:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.grimpatron:
                                        targethasdamageeffect = true; break;
                                    default:
                                        break;
                                }

                                switch (daum.bestmove.own.name)
                                {
                                    case CardDB.cardName.impgangboss:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.dragonegg:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.hoggerdoomofelwynn:
                                        targethasdamageeffect = true; break;
                                    case CardDB.cardName.grimpatron:
                                        targethasdamageeffect = true; break;
                                    default:
                                        break;
                                }

                                if (targethasdamageeffect && !daum.bestmove.target.isHero)
                                {
                                    System.Threading.Thread.Sleep(5200);
                                    //Helpfunctions.Instance.logg("타겟 맞을떄마다 소환합니다 sleep 3500ms");
                                    //Helpfunctions.Instance.logg("타겟 맞을떄마다 소환합니다 3500ms");
                                    //Helpfunctions.Instance.ErrorLog("타겟 맞을떄마다 소환합니다 1200ms");
                                    //Helpfunctions.Instance.ErrorLog("타겟 맞을떄마다 소환합니다 1200ms");
                                }






                                bool hastargetdeathrattle = (daum.bestmove.target.hasDeathrattle() || daum.bestmove.target.deathrattles.Count >= 1 || (daum.bestmove.target.handcard.card.deathrattle && !daum.bestmove.target.silenced)) && !daum.bestmove.target.isHero;
                                //Helpfunctions.Instance.logg("hasdeathrattle " + hastargetdeathrattle);

                                bool hasowndeathrattle = (daum.bestmove.own.hasDeathrattle() || daum.bestmove.own.deathrattles.Count >= 1 || (daum.bestmove.own.handcard.card.deathrattle && !daum.bestmove.own.silenced)) && !daum.bestmove.target.isHero;
                                //Helpfunctions.Instance.logg("hasdeathrattle " + hasowndeathrattle);




                                if (hastargetdeathrattle && daum.bestmove.own.Angr >= daum.bestmove.target.Hp && !daum.bestmove.target.isHero)
                                {
                                    System.Threading.Thread.Sleep(3500);
                                    Helpfunctions.Instance.logg("Target deathrattle detected sleep 3500ms");
                                    Helpfunctions.Instance.ErrorLog("Target deathrattle detected sleep 3500ms");
                                }

                                if (hasowndeathrattle && daum.bestmove.own.Hp <= daum.bestmove.target.Angr && !daum.bestmove.target.isHero)
                                {
                                    bool spawnMinions = false;
                                    bool hasDeathrattleBeast = false;
                                    switch (daum.bestmove.own.name)
                                    {
                                        case CardDB.cardName.harvestgolem: spawnMinions = true; break;
                                        case CardDB.cardName.hauntedcreeper: spawnMinions = true; break;
                                        case CardDB.cardName.nerubianegg: spawnMinions = true; break;
                                        case CardDB.cardName.savannahhighmane: spawnMinions = true; hasDeathrattleBeast = true; break;
                                        case CardDB.cardName.sludgebelcher: spawnMinions = true; break;
                                        case CardDB.cardName.thebeast: spawnMinions = true; break;
                                        case CardDB.cardName.cairnebloodhoof: spawnMinions = true; break;
                                        case CardDB.cardName.feugen: spawnMinions = true; break;
                                        case CardDB.cardName.stalagg: spawnMinions = true; break;
                                        case CardDB.cardName.infestedtauren: spawnMinions = true; break;
                                        case CardDB.cardName.infestedwolf: spawnMinions = true; hasDeathrattleBeast = true; break;
                                        case CardDB.cardName.dreadsteed: spawnMinions = true; break;
                                        case CardDB.cardName.voidcaller: spawnMinions = true; break;
                                        case CardDB.cardName.pilotedshredder: spawnMinions = true; break;
                                        case CardDB.cardName.pilotedskygolem: spawnMinions = true; break;
                                        case CardDB.cardName.mountedraptor: spawnMinions = true; break;
                                        case CardDB.cardName.sneedsoldshredder: spawnMinions = true; break;
                                        case CardDB.cardName.anubarak: spawnMinions = true; break;
                                        case CardDB.cardName.possessedvillager: spawnMinions = true; break;
                                        case CardDB.cardName.twilightsummoner: spawnMinions = true; break;
                                        case CardDB.cardName.wobblingrunts: spawnMinions = true; break;
                                        case CardDB.cardName.moatlurker: spawnMinions = true; break;
                                        case CardDB.cardName.kindlygrandmother: spawnMinions = true; hasDeathrattleBeast = true; break;
                                        default:
                                            break;
                                    }
                                    foreach (Minion mnn in this.ownMinions)
                                    {
                                        if (mnn.name == CardDB.cardName.knifejuggler && !mnn.silenced && spawnMinions)
                                        {
                                            System.Threading.Thread.Sleep(1500);
                                            Helpfunctions.Instance.logg("저글러 own minion's deathrattle detected sleep 1500ms");
                                            Helpfunctions.Instance.ErrorLog("저글러 own minion's deathrattle detected sleep 1500ms");
                                        }
                                        if (mnn.name == CardDB.cardName.tundrarhino && !mnn.silenced && hasDeathrattleBeast)
                                        {
                                            System.Threading.Thread.Sleep(500);
                                            Helpfunctions.Instance.logg("라이노 own minion's deathrattle detected sleep 500ms");
                                            Helpfunctions.Instance.ErrorLog("라이노 own minion's deathrattle detected sleep 500ms");
                                        }
                                    }


                                    System.Threading.Thread.Sleep(1500);
                                    Helpfunctions.Instance.logg("own minion's deathrattle detected sleep 1500ms");
                                    Helpfunctions.Instance.ErrorLog("own minion's deathrattle detected sleep 1500ms");
                                }

                                //if (daum.bestmove.target.taunt && daum.bestmove.own.Angr >= daum.bestmove.target.Hp && !daum.bestmove.target.isHero)
                                //{
                                //    System.Threading.Thread.Sleep(500);
                                //    Helpfunctions.Instance.logg("Target Taunt detected sleep 500ms");
                                //    Helpfunctions.Instance.logg("Target Taunt detected sleep 500ms");
                                //    Helpfunctions.Instance.ErrorLog("Target Taunt detected sleep 500ms");
                                //    Helpfunctions.Instance.ErrorLog("Target Taunt detected sleep 500ms");
                                //}


                                bool hashyena = false;
                                foreach (Minion mnn in this.ownMinions)
                                {
                                    if (mnn.name == CardDB.cardName.scavenginghyena && !mnn.silenced && mnn.Ready)
                                    {
                                        hashyena = true;
                                    }
                                }

                                if (hashyena && daum.bestmove.own.Hp <= daum.bestmove.target.Angr && !daum.bestmove.target.isHero && (TAG_RACE)daum.bestmove.own.handcard.card.race == TAG_RACE.BEAST)
                                {
                                    System.Threading.Thread.Sleep(1500);
                                }






                                //if (this.enemySecretCount >= 1)
                                //{
                                //    int time = 6000 / this.enemySecretCount;
                                //    foreach (SecretItem si in Probabilitymaker.Instance.enemySecrets)
                                //    {
                                //        if (si.canBe_noblesacrifice
                                //       || si.canBe_freezing)
                                //        {
                                //            System.Threading.Thread.Sleep(time * 4 / 3);
                                //            Helpfunctions.Instance.logg("찾는거 덫발견 덫1 슬립 time: " + time * 4 / 3);
                                //            Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫1 time: " + time * 4 / 3);
                                //        }

                                //        else if (daum.bestmove.target.isHero)
                                //        {
                                //            if (si.canBe_explosive
                                //                || si.canBe_beartrap
                                //                //|| si.canBe_icebarrier
                                //                || si.canBe_vaporize)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫2 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫2 time: " + time);
                                //            }
                                //        }
                                //        else if (!daum.bestmove.target.isHero)
                                //        {
                                //            if (si.canBe_snaketrap)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫3 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫3 time: " + time);
                                //            }
                                //            else if (daum.bestmove.own.Angr >= daum.bestmove.target.Hp)
                                //            {
                                //                if (si.canBe_iceblock)
                                //                {
                                //                    System.Threading.Thread.Sleep(time);
                                //                    Helpfunctions.Instance.logg("찾는거 덫발견 덫13 time: " + time);
                                //                    Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫13 time: " + time);
                                //                }
                                //            }
                                //        }
                                //        else if (daum.bestmove.own.Angr >= daum.bestmove.target.Hp)
                                //        {
                                //            if (si.canBe_effigy
                                //                || si.canBe_redemption
                                //                || si.canBe_avenge
                                //                || si.canBe_duplicate)
                                //            {
                                //                System.Threading.Thread.Sleep(time);
                                //                Helpfunctions.Instance.logg("찾는거 덫발견 덫7 time: " + time);
                                //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫7 time: " + time);
                                //            }
                                //        }
                                //    }
                                        

                                //}
                            }

                            
                            break;
                        }
                    case actionEnum.playcard:

                        if (rangerbot.gameState.TimerState != TurnTimerState.COUNTDOWN)
                        {
                            //if (this.enemySecretCount >= 1)
                            //{
                            //    int time = 6000 / this.enemySecretCount;
                            //    foreach (SecretItem si in Probabilitymaker.Instance.enemySecrets)
                            //    {
                            //        if (daum.bestmove.card.card.type == CardDB.cardtype.MOB)
                            //        {
                            //            if (si.canBe_mirrorentity)
                            //            {
                            //                System.Threading.Thread.Sleep(time);
                            //                Helpfunctions.Instance.logg("찾는거 덫발견 덫4 time: " + time);
                            //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫4 time: " + time);
                            //            }
                            //            else if ((si.canBe_snipe)
                            //                    || (si.canBe_Trial && this.ownMinions.Count >= 3))
                            //            {
                            //                System.Threading.Thread.Sleep(time);
                            //                Helpfunctions.Instance.logg("찾는거 덫발견 덫5 time: " + time);
                            //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫5 time: " + time);
                            //            }
                            //            //else if (daum.bestmove.card.card.Charge)
                            //            //{
                            //            //    if ((si.canBe_snipe && daum.bestmove.card.card.Health <= 4)
                            //            //        || (si.canBe_Trial && this.ownMinions.Count >= 3))
                            //            //    {
                            //            //        System.Threading.Thread.Sleep(time);
                            //            //        Helpfunctions.Instance.logg("찾는거 덫발견 덫5 time: " + time);
                            //            //        Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫5 time: " + time);
                            //            //    }
                            //            //}
                            //        }
                            //        else if (daum.bestmove.card.card.type == CardDB.cardtype.SPELL)
                            //        {
                            //            if (si.canBe_counterspell
                            //                    || (si.canBe_spellbender && daum.bestmove.target != null && !daum.bestmove.target.isHero)
                            //                    || si.canBe_cattrick)
                            //            {
                            //                System.Threading.Thread.Sleep(time);
                            //                Helpfunctions.Instance.logg("찾는거 덫발견 덫6 time: " + time);
                            //                Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫6 time: " + time);
                            //            }
                            //            else if (this.enemyMinions.Count >= 1 && 
                            //                (PenalityManager.Instance.DamageAllDatabase.ContainsKey(daum.bestmove.card.card.name)
                            //                || PenalityManager.Instance.DamageRandomDatabase.ContainsKey(daum.bestmove.card.card.name)
                            //                || PenalityManager.Instance.DamageAllEnemysDatabase.ContainsKey(daum.bestmove.card.card.name)
                            //                || PenalityManager.Instance.DamageAllDatabase.ContainsKey(daum.bestmove.card.card.name)
                            //                || PenalityManager.Instance.DamageTargetDatabase.ContainsKey(daum.bestmove.card.card.name)
                            //                || PenalityManager.Instance.DamageTargetSpecialDatabase.ContainsKey(daum.bestmove.card.card.name)))
                            //            {
                            //                if (si.canBe_effigy
                            //                    || si.canBe_redemption
                            //                    || si.canBe_avenge
                            //                    || si.canBe_duplicate)
                            //                {
                            //                    System.Threading.Thread.Sleep(time);
                            //                    Helpfunctions.Instance.logg("찾는거 덫발견 덫7 time: " + time);
                            //                    Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫7 time: " + time);
                            //                }
                            //            }
                            //        }
                            //    }
                            //}

                            if (daum.bestmove.card.card.battlecry)
                            {
                                //Helpfunctions.Instance.logg("배틀크라이 발견!!!!!!!");
                                //Helpfunctions.Instance.ErrorLog("배틀크라이 발견!!!!!!!");

                                switch (daum.bestmove.card.card.name)
                                {
                                    case CardDB.cardName.defenderofargus: System.Threading.Thread.Sleep(2200); break;
                                    case CardDB.cardName.kingselekk: System.Threading.Thread.Sleep(8200); break;
                                    case CardDB.cardName.barnes: System.Threading.Thread.Sleep(5200); break;
                                    case CardDB.cardName.tuskarrtotemic: System.Threading.Thread.Sleep(5500); break;
                                    case CardDB.cardName.thecurator: System.Threading.Thread.Sleep(4200); break;
                                    default: break;
                                }
                                System.Threading.Thread.Sleep(2300);
                            }
                            else if (daum.bestmove.card.card.Charge)
                            {
                                System.Threading.Thread.Sleep(2200);
                            }
                            else
                            {
                                switch (daum.bestmove.card.card.name)
                                {
                                    case CardDB.cardName.southseadeckhand:
                                        if (this.heroWeaponAttack >= 1)
                                        {
                                            System.Threading.Thread.Sleep(3200);
                                        }
                                        break;
                                    //case CardDB.cardName.: System.Threading.Thread.Sleep(2200); break;
                                    default: break;
                                }
                            }

                            if (daum.bestmove.card.card.type == CardDB.cardtype.MOB || PenalityManager.Instance.summonMinionSpellsDatabase.ContainsKey(daum.bestmove.card.card.name))
                            {
                                int juggler_councilman_count = 0;
                                bool juggleronfield = false;
                                foreach (Minion mnn in this.ownMinions)
                                {
                                    if (!mnn.silenced && ((mnn.name == CardDB.cardName.darkshirecouncilman && !mnn.playedThisTurn) || mnn.name == CardDB.cardName.knifejuggler))
                                    {
                                        juggler_councilman_count++;
                                        if (mnn.name == CardDB.cardName.knifejuggler) juggleronfield = true;
                                    }
                                }
                                if (juggler_councilman_count >= 1 && this.enemyMinions.Count >= 1)
                                {
                                    int time = 2100 * juggler_councilman_count;
                                    if (juggleronfield) time = time *  13/ 10;
                                    if (daum.bestmove.card.card.name == CardDB.cardName.forbiddenritual)
                                    {
                                        time = time * (Math.Min(7 - this.ownMinions.Count, this.currentMana));
                                        System.Threading.Thread.Sleep(time);
                                        //Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                    }
                                    else if (daum.bestmove.card.card.name == CardDB.cardName.unleashthehounds)
                                    {
                                        time = time * (Math.Min(7 - this.ownMinions.Count, this.enemyMinions.Count));
                                        time = time * 13 / 10;
                                        System.Threading.Thread.Sleep(time);
                                        //Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                        //Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep" + time + "ms");
                                    }
                                    else if (PenalityManager.Instance.summonMinionSpellsDatabase.ContainsKey(daum.bestmove.card.card.name))
                                    {
                                        time = time * daum.bestmove.card.card.Summon_Spell_Minion_Count;
                                        System.Threading.Thread.Sleep(time);
                                        Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep " + time + "ms" + daum.bestmove.card.card.Summon_Spell_Minion_Count + "enemy minions");
                                        Helpfunctions.Instance.logg("darkshirecouncilman or knifejuggler effect detected sleep " + time + "ms" + daum.bestmove.card.card.Summon_Spell_Minion_Count + "enemy minions");
                                        Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep " + time + "ms" + daum.bestmove.card.card.Summon_Spell_Minion_Count + "enemy minions");
                                        Helpfunctions.Instance.ErrorLog("darkshirecouncilman or knifejuggler effect detected sleep " + time + "ms" + daum.bestmove.card.card.Summon_Spell_Minion_Count + "enemy minions");
                                    }
                                    else
                                    {
                                        System.Threading.Thread.Sleep(time);
                                    }
                                    break;
                                }
                            }


                            if (daum.bestmove.card.card.type == CardDB.cardtype.SPELL)
                            {
                                System.Threading.Thread.Sleep(1200);

                                if (daum.bestmove.target != null)
                                {
                                    bool hastargetdeathrattle = (daum.bestmove.target.hasDeathrattle() || daum.bestmove.target.deathrattles.Count >= 1 || (daum.bestmove.target.handcard.card.deathrattle && !daum.bestmove.target.silenced)) && !daum.bestmove.target.isHero;
                                    if (hastargetdeathrattle)
                                    {
                                        System.Threading.Thread.Sleep(3500);
                                    }

                                    if (PenalityManager.Instance.NeedSleepBecauseTimingMinionsDB.ContainsKey(daum.bestmove.target.name))
                                    {
                                        int timing = PenalityManager.Instance.NeedSleepBecauseTimingMinionsDB[daum.bestmove.target.name] * 1050;
                                        System.Threading.Thread.Sleep(timing);
                                    }

                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.callofthewild)
                                {
                                    System.Threading.Thread.Sleep(2200);
                                }
                                //적시바 나 데드리샷

                                if (daum.bestmove.card.card.name == CardDB.cardName.deadlyshot)
                                {
                                    foreach (Minion mnn in this.enemyMinions)
                                    {
                                        if (mnn.name == CardDB.cardName.sylvanaswindrunner && !mnn.silenced)
                                        {
                                            System.Threading.Thread.Sleep(4500);
                                        }
                                    }
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.quickshot && this.anzcards == 1)
                                {
                                    int pow = 3200;
                                    System.Threading.Thread.Sleep(pow);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.lightningstorm)
                                {
                                    int pow = 2200;
                                    if (this.enemyMinions.Count >= 1) pow = pow * this.enemyMinions.Count * 3 / 4;
                                    System.Threading.Thread.Sleep(pow);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.lightningbolt)
                                {
                                    System.Threading.Thread.Sleep(1500);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.feralspirit)
                                {
                                    System.Threading.Thread.Sleep(4500);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.rockbiterweapon)
                                {
                                    System.Threading.Thread.Sleep(1500);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.maelstromportal)
                                {
                                    System.Threading.Thread.Sleep(2500);
                                }
                                if (daum.bestmove.card.card.name == CardDB.cardName.hex)
                                {
                                    System.Threading.Thread.Sleep(1500);
                                }

                                switch (daum.bestmove.card.card.name)
                                {
                                    case CardDB.cardName.upgrade: System.Threading.Thread.Sleep(1500); break;
                                    default: break;
                                }
                            }

                            //shaman 1cost trogg
                            bool hastrogg = false;
                            foreach (Minion mnn in this.ownMinions)
                            {
                                if (mnn.name == CardDB.cardName.tunneltrogg && !mnn.silenced && mnn.Ready)
                                {
                                    hastrogg = true;
                                }
                            }
                            if (hastrogg && daum.bestmove.card.card.overload >= 1) //overload
                            {
                                System.Threading.Thread.Sleep(1500);
                                Helpfunctions.Instance.logg("찾는거 오버로드 " + daum.bestmove.card.card.name);
                                Helpfunctions.Instance.ErrorLog("찾는거 오버로드 " + daum.bestmove.card.card.name);
                            }



                        }

                            

                        break;
                    case actionEnum.useHeroPower:
                        //if (this.enemySecretCount >= 1)
                        //{
                        //    int time = 6000 / this.enemySecretCount;
                        //    foreach (SecretItem si in Probabilitymaker.Instance.enemySecrets)
                        //    {
                        //        if (si.canBe_Dart)
                        //        {
                        //            System.Threading.Thread.Sleep(time);
                        //            Helpfunctions.Instance.logg("찾는거 덫발견 덫21 time: " + time);
                        //            Helpfunctions.Instance.ErrorLog("찾는거 덫발견 덫21 time: " + time);
                        //        }
                        //    }
                        //}
                        //if (heroAbility.name == CardDB.cardName.shapeshift || heroAbility.name == CardDB.cardName.direshapeshift) System.Threading.Thread.Sleep(1200);
                        //if (rangerbot.gameState.TimerState != TurnTimerState.COUNTDOWN) System.Threading.Thread.Sleep(1800);
                        break;
                    default:
                        break;
                }
            }


            Helpfunctions.Instance.ErrorLog("updateEverything");
            latestGameState = rangerbot.gameState;

            this.updateBehaveString(botbase);

            Entity ownPlayer = rangerbot.FriendPlayer;
            Entity enemyPlayer = rangerbot.EnemyPlayer;
            ownPlayerController = ownPlayer.ControllerId;//ownPlayer.GetHero().GetControllerId()

            Hrtprozis.Instance.clearAll();
            Handmanager.Instance.clearAll();

            // create hero + minion data
            getHerostuff(rangerbot);
            getMinions(rangerbot);
            getHandcards(rangerbot);
            getDecks(rangerbot);
            correctSpellpower(rangerbot);

            Hrtprozis.Instance.setOwnPlayer(ownPlayerController);
            Handmanager.Instance.setOwnPlayer(ownPlayerController);

            this.numOptionPlayedThisTurn = 0;
            this.numOptionPlayedThisTurn += this.cardsPlayedThisTurn + this.ownHero.numAttacksThisTurn;
            foreach (Minion m in this.ownMinions)
            {
                if (m.Hp >= 1) this.numOptionPlayedThisTurn += m.numAttacksThisTurn;
            }

            Hrtprozis.Instance.updatePlayer(this.ownMaxMana, this.currentMana, this.cardsPlayedThisTurn, this.numMinionsPlayedThisTurn, this.numOptionPlayedThisTurn, this.ownOverload, ownHero.entityID, enemyHero.entityID, this.numberMinionsDiedThisTurn, this.ownCurrentOverload, this.enemyOverload, this.heroPowerUsesThisTurn,this.lockandload,this.Stampede);
            Hrtprozis.Instance.setPlayereffects(this.ownDragonConsort, this.enemyDragonConsort, this.ownLoathebs, this.enemyLoathebs, this.ownMillhouse, this.enemyMillhouse, this.ownKirintor, this.ownPrepa, this.ownsabo, this.enemysabo, this.ownFenciCoaches, this.enemyCursedCardsInHand);
            Hrtprozis.Instance.updateSecretStuff(this.ownSecretList, this.enemySecretCount);


            Hrtprozis.Instance.updateOwnHero(this.ownHeroWeapon, this.heroWeaponAttack, this.heroWeaponDurability, this.heroname, this.heroAbility, this.ownAbilityisReady, this.ownHero, this.ownHeroPowerUsesThisGame);
            Hrtprozis.Instance.updateEnemyHero(this.enemyHeroWeapon, this.enemyWeaponAttack, this.enemyWeaponDurability, this.enemyHeroname, this.enemyMaxMana, this.enemyAbility, this.enemyHero, this.enemyHeroPowerUsesThisGame);

            Hrtprozis.Instance.updateMinions(this.ownMinions, this.enemyMinions);
            Handmanager.Instance.setHandcards(this.handCards, this.anzcards, this.enemyAnzCards, this.choiceCards);

            Hrtprozis.Instance.updateFatigueStats(this.ownDecksize, this.ownHeroFatigue, this.enemyDecksize, this.enemyHeroFatigue);
            Hrtprozis.Instance.updateJadeGolemsInfo(ownPlayer.GetTagValue((int)GAME_TAG.JADE_GOLEM), enemyPlayer.GetTagValue((int)GAME_TAG.JADE_GOLEM));
            Probabilitymaker.Instance.getEnemySecretGuesses(this.enemySecretList, Hrtprozis.Instance.heroNametoEnum(this.enemyHeroname));

            //Hrtprozis.Instance.updateCrystalCore(OwnCrystalCore, EnemyCrystalCore);
            Hrtprozis.Instance.updateOwnMinionsCost0(this.ownMinionsCost0);

            //learnmode :D

            Playfield p = new Playfield();


            if (!queueActions)
            {
                if (lastpf != null)
                {
                    if (lastpf.isEqualf(p))
                    {
                        return false;
                    }
                    
                    //board changed we update secrets!
                    //if(Ai.Instance.nextMoveGuess!=null) Probabilitymaker.Instance.updateSecretList(Ai.Instance.nextMoveGuess.enemySecretList);
                    Probabilitymaker.Instance.updateSecretList(p, lastpf);
                }
            }
            else
            {
                //queue stuff 
                if (lastpf != null)
                {
                    if (lastpf.isEqualf(p))
                    {
                        ((Bot)rangerbot).shouldSendActions = false;  // let the bot know we haven't updated any actions
                        return false;
                    }

                    //board changed we update secrets!
                    //if(Ai.Instance.nextMoveGuess!=null) Probabilitymaker.Instance.updateSecretList(Ai.Instance.nextMoveGuess.enemySecretList);
                    Probabilitymaker.Instance.updateSecretList(p, lastpf);
                }

            }


            lastpf = p;
            p = new Playfield();//secrets have updated :D
            // calculate stuff

            /*foreach (Handmanager.Handcard hc in p.owncards)
            {
                Helpfunctions.Instance.ErrorLog("hc playfield" + hc.manacost + " " + hc.getManaCost(p));
            }*/

            if (queueActions)
            {
                // Detect errors in HearthRanger execution of our last set of actions and try to fix it so we don't
                // have to re-calculate the entire turn.
                Bot currentBot = (Bot)rangerbot;
                if (currentBot.numActionsSent > currentBot.numExecsReceived && !p.isEqualf(Ai.Instance.nextMoveGuess))
                {
                    Helpfunctions.Instance.ErrorLog("HR action queue did not complete!");
                    Helpfunctions.Instance.logg("board state out-of-sync due to action queue!");

                    //if (Ai.Instance.restoreBestMoves(p, currentBot.queuedMoveGuesses))
                    //{
                    //    Helpfunctions.Instance.logg("rolled back state to replay queued actions.");
                    //    Helpfunctions.Instance.ErrorLog("#queue-rollback#");
                    //}
                }
            }
            if (p.mana > Ai.Instance.nextMoveGuess.mana && p.ownMaxMana > Ai.Instance.nextMoveGuess.ownMaxMana && Ai.Instance.bestActions.Count > 0)
            {
                Helpfunctions.Instance.logg("You may have roped last turn!");
                //Helpfunctions.Instance.logg("Mana: " + p.mana + ">" + Ai.Instance.nextMoveGuess.mana);
                //Helpfunctions.Instance.logg("Max Mana: " + p.ownMaxMana + ">" + Ai.Instance.nextMoveGuess.ownMaxMana);
                //Helpfunctions.Instance.logg("Actions left: " + Ai.Instance.bestActions.Count);
            }

            Helpfunctions.Instance.ErrorLog("calculating stuff... " + DateTime.Now.ToString("HH:mm:ss.ffff"));
            if (runExtern)
            {
                Helpfunctions.Instance.logg("recalc-check###########");
                //p.printBoard();
                //Ai.Instance.nextMoveGuess.printBoard();
                if (p.isEqual(Ai.Instance.nextMoveGuess, true))
                {
                    printstuff(p, false);
                    Ai.Instance.doNextCalcedMove();
                }
                else
                {
                    List < Handmanager.Handcard > newcards = p.getNewHandCards(Ai.Instance.nextMoveGuess);
                    foreach (var card in newcards)
                    {
                        if (!isCardCreated(card)) Hrtprozis.Instance.removeCardFromTurnDeck(card.card.cardIDenum);
                    }

                    Bot.Instance.dontmultiactioncount = 0;
                    Bot.Instance.POWERFULSINGLEACTION = 0;
                    printstuff(p, true);
                    readActionFile(passiveWait);
                }
            }
            else
            {
                printstuff(p, false);
                Ai.Instance.dosomethingclever(botbase);
            }

            Helpfunctions.Instance.ErrorLog("calculating ended! " + DateTime.Now.ToString("HH:mm:ss.ffff"));

            return true;
        }

        public bool isCardCreated(Handmanager.Handcard handcard)
        {
            foreach (var card in latestGameState.GameEntityList)
            {
                if (card.EntityId == handcard.entity)
                {
                    if (card.CreatorId != 0) return true;
                    else return false;
                }
            }
            return false;
        }


        private void getHerostuff(HSRangerLib.BotBase rangerbot)
        {

            //TODO GET HERO POWER USES!!!!!!
            //heroPowerUsesThisTurn = 0;
            //ownHeroPowerUsesThisGame = 0;
            //enemyHeroPowerUsesThisGame = 0;

            //reset playerbuffs (thx to xytri)
            this.enemyMillhouse = 0;
            this.enemyLoathebs = 0;
            this.ownDragonConsort = 0;
            this.ownKirintor = 0;
            this.ownPrepa = 0;
            this.lockandload = 0;
            this.Stampede = 0;
            this.enemysabo = 0;
            this.ownFenciCoaches = 0;
            this.ownMillhouse = 0;
            this.ownLoathebs = 0;
            this.enemyDragonConsort = 0;
            this.ownsabo = 0;


            Dictionary<int, Entity> allEntitys = new Dictionary<int, Entity>();

            foreach (var item in rangerbot.gameState.GameEntityList)
            {
                allEntitys.Add(item.EntityId, item);
            }

            Entity ownhero = rangerbot.FriendHero;
            Entity enemyhero = rangerbot.EnemyHero;
            Entity ownHeroAbility = rangerbot.FriendHeroPower;

            //player stuff#########################
            //this.currentMana =ownPlayer.GetTag(HRGameTag.RESOURCES) - ownPlayer.GetTag(HRGameTag.RESOURCES_USED) + ownPlayer.GetTag(HRGameTag.TEMP_RESOURCES);
            this.currentMana = rangerbot.gameState.CurrentMana;
            this.ownMaxMana = rangerbot.gameState.LocalMaxMana;
            this.enemyMaxMana = rangerbot.gameState.RemoteMaxMana;
            //enemySecretCount = rangerbot.EnemySecrets.Count;
            //enemySecretCount = 0;
            //count enemy secrets
            enemySecretList.Clear();

            foreach (var item in rangerbot.EnemySecrets)
            {
                if (item.GetTagValue((int)GAME_TAG.QUEST) >= 1)
                {
                    Questmanager.Instance.updateQuestStuff(item.CardId, item.GetTagValue((int)GAME_TAG.QUEST_PROGRESS), item.GetTagValue((int)GAME_TAG.QUEST_PROGRESS_TOTAL), false);
                    continue;
                }
                enemySecretList.Add(item.EntityId);
            }
            enemySecretCount = enemySecretList.Count;

            this.ownSecretList.Clear();
            foreach (var item in rangerbot.FriendSecrets)
            {
                if (item.GetTagValue((int)GAME_TAG.QUEST) >= 1)
                {
                    Questmanager.Instance.updateQuestStuff(item.CardId, item.GetTagValue((int)GAME_TAG.QUEST_PROGRESS), item.GetTagValue((int)GAME_TAG.QUEST_PROGRESS_TOTAL), true);
                    continue;
                }
                this.ownSecretList.Add(item.CardId);
            }

            this.numMinionsPlayedThisTurn = rangerbot.gameState.NumMinionsPlayedThisTurn;
            this.cardsPlayedThisTurn = rangerbot.gameState.NumCardsPlayedThisTurn;
            

            //get weapon stuff
            this.ownHeroWeapon = "";
            this.heroWeaponAttack = 0;
            this.heroWeaponDurability = 0;

            this.ownHeroFatigue = ownhero.Fatigue;
            this.enemyHeroFatigue = enemyhero.Fatigue;

            this.ownDecksize = rangerbot.gameState.LocalDeckRemain;
            this.enemyDecksize = rangerbot.gameState.RemoteDeckRemain;


            //own hero stuff###########################
            int heroAtk = ownhero.ATK;
            int heroHp = ownhero.Health - ownhero.Damage;
            int heroDefence = ownhero.Armor;
            this.heroname = Hrtprozis.Instance.heroIDtoName(ownhero.CardId);

            bool heroImmuneToDamageWhileAttacking = false;
            bool herofrozen = ownhero.IsFrozen;
            int heroNumAttacksThisTurn = ownhero.NumAttacksThisTurn;
            bool heroHasWindfury = ownhero.HasWindfury;
            bool heroImmune = (ownhero.IsImmune);

            //Helpfunctions.Instance.ErrorLog(ownhero.GetName() + " ready params ex: " + exausted + " " + heroAtk + " " + numberofattacks + " " + herofrozen);


            if (rangerbot.FriendWeapon != null)
            {
                Entity weapon = rangerbot.FriendWeapon;
                this.ownHeroWeapon = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(rangerbot.FriendWeapon.CardId)).name.ToString();
                this.heroWeaponAttack = weapon.ATK;
                this.heroWeaponDurability = weapon.Durability - weapon.Damage;//weapon.GetDurability();
                heroImmuneToDamageWhileAttacking = false;
                if (this.ownHeroWeapon == "gladiatorslongbow")
                {
                    heroImmuneToDamageWhileAttacking = true;
                }
                if (this.ownHeroWeapon == "doomhammer")
                {
                    heroHasWindfury = true;
                }

                //Helpfunctions.Instance.ErrorLog("weapon: " + ownHeroWeapon + " " + heroWeaponAttack + " " + heroWeaponDurability);

            }



            //enemy hero stuff###############################################################
            this.enemyHeroname = Hrtprozis.Instance.heroIDtoName(enemyhero.CardId);

            int enemyAtk = enemyhero.ATK;
            int enemyHp = enemyhero.Health - enemyhero.Damage;
            int enemyDefence = enemyhero.Armor;
            bool enemyfrozen = enemyhero.IsFrozen;
            bool enemyHeroImmune = (enemyhero.IsImmune);

            this.enemyHeroWeapon = "";
            this.enemyWeaponAttack = 0;
            this.enemyWeaponDurability = 0;
            if (rangerbot.EnemyWeapon != null)
            {
                Entity weapon = rangerbot.EnemyWeapon;
                this.enemyHeroWeapon = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(weapon.CardId)).name.ToString();
                this.enemyWeaponAttack = weapon.ATK;
                this.enemyWeaponDurability = weapon.Durability - weapon.Damage;
            }


            //own hero power stuff###########################################################

            this.heroAbility = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(ownHeroAbility.CardId));
            this.ownAbilityisReady = (ownHeroAbility.IsExhausted) ? false : true; // if exhausted, ability is NOT ready

            //only because hearthranger desnt give me the data ;_; use the tag HEROPOWER_ACTIVATIONS_THIS_TURN instead! (of own player)
            //this.heroPowerUsesThisTurn = 10000;
            //if (this.ownAbilityisReady) this.heroPowerUsesThisTurn = 0;
            this.heroPowerUsesThisTurn = rangerbot.gameState.HeroPowerActivationsThisTurn;
            this.ownHeroPowerUsesThisGame = rangerbot.gameState.NumTimesHeroPowerUsedThisGame;

            this.enemyAbility = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(rangerbot.EnemyHeroPower.CardId));

            //generate Heros
            this.ownHero = new Minion();
            this.enemyHero = new Minion();
            this.ownHero.isHero = true;
            this.enemyHero.isHero = true;
            this.ownHero.own = true;
            this.enemyHero.own = false;
            this.ownHero.maxHp = ownhero.Health;
            this.enemyHero.maxHp = enemyhero.Health;
            this.ownHero.entityID = ownhero.EntityId;
            this.enemyHero.entityID = enemyhero.EntityId;

            this.ownHero.Angr = heroAtk;
            this.ownHero.Hp = heroHp;
            this.ownHero.armor = heroDefence;
            this.ownHero.frozen = herofrozen;
            this.ownHero.immuneWhileAttacking = heroImmuneToDamageWhileAttacking;
            this.ownHero.immune = heroImmune;
            this.ownHero.numAttacksThisTurn = heroNumAttacksThisTurn;
            this.ownHero.windfury = heroHasWindfury;

            this.enemyHero.Angr = enemyAtk;
            this.enemyHero.Hp = enemyHp;
            this.enemyHero.frozen = enemyfrozen;
            this.enemyHero.armor = enemyDefence;
            this.enemyHero.immune = enemyHeroImmune;
            this.enemyHero.Ready = false;

            this.ownHero.updateReadyness();


            //load enchantments of the heros
            List<miniEnch> miniEnchlist = new List<miniEnch>();
            foreach (Entity ent in allEntitys.Values)
            {
                if (ent.Attached == this.ownHero.entityID && ent.Zone == HSRangerLib.TAG_ZONE.PLAY)
                {
                    CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                    int controler = ent.ControllerId;
                    int creator = ent.CreatorId;
                    miniEnchlist.Add(new miniEnch(id, creator, controler));
                }

            }

            this.ownHero.loadEnchantments(miniEnchlist, ownhero.ControllerId);

            miniEnchlist.Clear();

            foreach (Entity ent in allEntitys.Values)
            {
                if (ent.Attached == this.enemyHero.entityID && ent.Zone == HSRangerLib.TAG_ZONE.PLAY)
                {
                    CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                    int controler = ent.ControllerId;
                    int creator = ent.CreatorId;
                    miniEnchlist.Add(new miniEnch(id, creator, controler));
                }

            }

            this.enemyHero.loadEnchantments(miniEnchlist, enemyhero.ControllerId);
            //fastmode weapon correction:
            if (ownHero.Angr < this.heroWeaponAttack) ownHero.Angr = this.heroWeaponAttack;
            if (enemyHero.Angr < this.enemyWeaponAttack) enemyHero.Angr = this.enemyWeaponAttack;

            this.ownOverload = rangerbot.gameState.RecallOwnedNum;//was at the start, but copied it over here :D , its german for overload :D
            //Reading new values:###################################################################################################
            //ToDo:

            this.numberMinionsDiedThisTurn = rangerbot.gameState.NumMinionsKilledThisTurn;
            
            //this should work (hope i didnt oversee a value :D)

            this.ownCurrentOverload = rangerbot.gameState.RecalledCrystalsOwedNextTurn;// ownhero.GetTag(HRGameTag.RECALL);
            this.enemyOverload = 0;// enemyhero.GetTag(HRGameTag.RECALL_OWED);

            //count buffs off !!players!! (players and not heros) (like preparation, kirintor-buff and stuff)
            // hope this works, dont own these cards to test where its attached

            int owncontrollerblubb = ownhero.ControllerId + 1; // controller = 1 or 2, but entity with 1 is the board -> +1
            int enemycontrollerblubb = enemyhero.ControllerId + 1;// controller = 1 or 2, but entity with 1 is the board -> +1

            //will not work in Hearthranger!


            foreach (Entity ent in allEntitys.Values)
            {
                if (ent.Attached == owncontrollerblubb && ent.Zone == HSRangerLib.TAG_ZONE.PLAY) //1==play
                {
                    CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                    if (id == CardDB.cardIDEnum.NEW1_029t) this.enemyMillhouse++;//CHANGED!!!!
                    if (id == CardDB.cardIDEnum.FP1_030e) this.enemyLoathebs++; //CHANGED!!!!
                    if (id == CardDB.cardIDEnum.BRM_018e) this.ownDragonConsort++;
                    if (id == CardDB.cardIDEnum.EX1_612o) this.ownKirintor++;
                    if (id == CardDB.cardIDEnum.EX1_145o) this.ownPrepa++;
                    if (id == CardDB.cardIDEnum.AT_061e) this.lockandload++;
                    if (id == CardDB.cardIDEnum.UNG_916e) this.Stampede++;
                    if (id == CardDB.cardIDEnum.AT_086e) this.enemysabo++;
                    if (id == CardDB.cardIDEnum.AT_115e) this.ownFenciCoaches++;

                }

                if (ent.Attached == enemycontrollerblubb && ent.Zone == HSRangerLib.TAG_ZONE.PLAY) //1==play
                {
                    CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                    if (id == CardDB.cardIDEnum.NEW1_029t) this.ownMillhouse++; //CHANGED!!!! (enemy has the buff-> we played millhouse)
                    if (id == CardDB.cardIDEnum.FP1_030e) this.ownLoathebs++; //CHANGED!!!!
                    if (id == CardDB.cardIDEnum.BRM_018e) this.enemyDragonConsort++;
                    // not needef for enemy, because its lasting only for his turn
                    //if (id == CardDB.cardIDEnum.EX1_612o) this.enemyKirintor++;
                    //if (id == CardDB.cardIDEnum.EX1_145o) this.enemyPrepa++;
                    if (id == CardDB.cardIDEnum.AT_086e) this.ownsabo++;
                }

            }
            this.lockandload = (rangerbot.gameState.LocalPlayerLockAndLoad)? 1 : 0;
            //this.Stampede = (rangerbot.gameState.LocalPlayerLockAndLoad) ? 1 : 0;

            //saboteur test:
            if (ownHeroAbility.Cost >= 3) Helpfunctions.Instance.ErrorLog("heroabilitymana " + ownHeroAbility.Cost);
            if (this.enemysabo == 0 && ownHeroAbility.Cost >= 3) this.enemysabo++;
            if (this.enemysabo == 1 && ownHeroAbility.Cost >= 8) this.enemysabo++;

            //TODO test Bolvar Fordragon but it will be on his card :D
            //Reading new values end################################

        }

        private void getMinions(HSRangerLib.BotBase rangerbot)
        {
            Dictionary<int, Entity> allEntitys = new Dictionary<int, Entity>();
            
            //TEST....................
            /*
            Helpfunctions.Instance.ErrorLog("# all");
            foreach (var item in rangerbot.gameState.GameEntityList)
            {
                allEntitys.Add(item.EntityId, item);
                Helpfunctions.Instance.ErrorLog(item.CardId + " e " + item.EntityId + " a " + item.Attached + " controler " + item.ControllerId + " creator " + item.CreatorId + " zone " + item.Zone + " zp " + item.ZonePosition);
                List<Entity> ents = item.Attachments;
                foreach (var item1 in ents)
                {
                    Helpfunctions.Instance.ErrorLog("#" + item1.CardId + " e " + item1.EntityId + " a " + item1.Attached + " controler " + item1.ControllerId + " creator " + item1.CreatorId + " zone " + item1.Zone);
                }
            }*/

            ownMinions.Clear();
            enemyMinions.Clear();
            Entity ownPlayer = rangerbot.FriendHero;
            Entity enemyPlayer = rangerbot.EnemyHero;

            // ALL minions on Playfield:
            List<Entity> list = new List<Entity>();

            foreach (var item in rangerbot.FriendMinion)
            {
                list.Add(item);
            }

            foreach (var item in rangerbot.EnemyMinion)
            {
                list.Add(item);
            }


            List<Entity> enchantments = new List<Entity>();


            foreach (Entity item in list)
            {
                Entity entity = item;
                int zp = entity.ZonePosition;

                if ((TAG_CARDTYPE)entity.CardType == TAG_CARDTYPE.MINION && zp >= 1)
                {
                    //Helpfunctions.Instance.ErrorLog("zonepos " + zp);
                    CardDB.Card c = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(entity.CardId));
                    Minion m = new Minion();
                    m.name = c.name;
                    m.handcard.card = c;
                    m.Angr = entity.ATK;
                    m.maxHp = entity.Health;
                    m.Hp = m.maxHp - entity.Damage;
                    if (m.Hp <= 0) continue;
                    m.wounded = false;
                    if (m.maxHp > m.Hp) m.wounded = true;


                    m.exhausted = entity.IsExhausted;

                    m.taunt = (entity.HasTaunt);

                    m.numAttacksThisTurn = entity.NumAttacksThisTurn;

                    int temp = entity.NumTurnsInPlay;
                    m.playedThisTurn = (temp == 0) ? true : false;

                    m.windfury = (entity.HasWindfury);

                    m.frozen = (entity.IsFrozen);

                    m.divineshild = (entity.HasDivineShield);

                    m.stealth = (entity.IsStealthed);

                    m.poisonous = (entity.IsPoisonous);

                    m.immune = (entity.IsImmune);

                    m.silenced = entity.IsSilenced;

                    m.spellpower = entity.SpellPower;

                    m.charge = 0;

                    if (!m.silenced && m.name == CardDB.cardName.southseadeckhand && entity.HasCharge) m.charge = 1;
                    if (!m.silenced && m.handcard.card.Charge) m.charge = 1;
                    if (m.charge == 0 && entity.HasCharge) m.charge = 1;
                    m.zonepos = zp;

                    m.entityID = entity.EntityId;

                    if(m.name == CardDB.cardName.unknown) Helpfunctions.Instance.ErrorLog("unknown card error");

                    Helpfunctions.Instance.ErrorLog(m.entityID + " ." + entity.CardId + ". " + m.name + " ready params ex: " + m.exhausted + " charge: " + m.charge + " attcksthisturn: " + m.numAttacksThisTurn + " playedthisturn " + m.playedThisTurn);
                    //Helpfunctions.Instance.ErrorLog("spellpower check " + entitiy.SpellPowerAttack + " " + entitiy.SpellPowerHealing + " " + entitiy.SpellPower);


                    List<miniEnch> enchs = new List<miniEnch>();
                    /*foreach (Entity ent in allEntitys.Values)
                    {
                        if (ent.Attached == m.entitiyID && ent.Zone == HSRangerLib.TAG_ZONE.PLAY)
                        {
                            CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                            int creator = ent.CreatorId;
                            int controler = ent.ControllerId;
                            enchs.Add(new miniEnch(id, creator, controler));
                        }

                    }*/

                    foreach (Entity ent in item.Attachments)
                    {
                        CardDB.cardIDEnum id = CardDB.Instance.cardIdstringToEnum(ent.CardId);
                        int creator = ent.CreatorId;
                        int controler = ent.ControllerId;
                        enchs.Add(new miniEnch(id, creator, controler));

                    }

                    m.loadEnchantments(enchs, entity.ControllerId);




                    m.Ready = false; // if exhausted, he is NOT ready

                    m.updateReadyness();


                    if (entity.ControllerId == this.ownPlayerController) // OWN minion
                    {
                        m.own = true;
                        this.ownMinions.Add(m);
                    }
                    else
                    {
                        m.own = false;
                        this.enemyMinions.Add(m);
                    }

                }
                // minions added

                /*
                if (entitiy.GetCardType() == HRCardType.WEAPON)
                {
                    //Helpfunctions.Instance.ErrorLog("found weapon!");
                    if (entitiy.GetControllerId() == this.ownPlayerController) // OWN weapon
                    {
                        this.ownHeroWeapon = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(entitiy.GetCardId())).name.ToString();
                        this.heroWeaponAttack = entitiy.GetATK();
                        this.heroWeaponDurability = entitiy.GetDurability();
                        //this.heroImmuneToDamageWhileAttacking = false;


                    }
                    else
                    {
                        this.enemyHeroWeapon = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(entitiy.GetCardId())).name.ToString();
                        this.enemyWeaponAttack = entitiy.GetATK();
                        this.enemyWeaponDurability = entitiy.GetDurability();
                    }
                }

                if (entitiy.GetCardType() == HRCardType.ENCHANTMENT)
                {

                    enchantments.Add(entitiy);
                }
                 */


            }

            /*foreach (HRCard item in list)
            {
                foreach (HREntity e in item.GetEntity().GetEnchantments())
                {
                    enchantments.Add(e);
                }
            }


            // add enchantments to minions
            setEnchantments(enchantments);*/
        }

        private void correctSpellpower(HSRangerLib.BotBase rangerbot)
        {
            int ownspellpower = rangerbot.gameState.LocalPlayerSpellPower;
            int spellpowerfield = 0;
            int numberDalaranAspirant=0;
            foreach (Minion mnn in this.ownMinions)
            {
                if(mnn.name == CardDB.cardName.dalaranaspirant) numberDalaranAspirant++;
                spellpowerfield += mnn.spellpower;
            }
            int missingSpellpower = ownspellpower - spellpowerfield;
            if (missingSpellpower != 0 )
            {
                Helpfunctions.Instance.ErrorLog("spellpower correction: " + ownspellpower + " " + spellpowerfield + " " + numberDalaranAspirant);
            }
            if (missingSpellpower >= 1 && numberDalaranAspirant >= 1)
            {
                //give all dalaran aspirants the "same amount" of spellpower
                for (int i = 0; i < missingSpellpower; i++)
                {
                    Minion dalaranAspriant = null;
                    int spellpower = ownspellpower;

                    foreach (Minion mnn in this.ownMinions)
                    {
                        if (mnn.name == CardDB.cardName.dalaranaspirant)
                        {
                            if (spellpower >= mnn.spellpower)
                            {
                                spellpower = mnn.spellpower;
                                dalaranAspriant = mnn;
                            }
                        }
                    }
                    dalaranAspriant.spellpower++;
                }

            }
        }

        private void setEnchantments(List<Entity> enchantments)
        {
            /*
            foreach (HREntity bhu in enchantments)
            {
                //create enchantment
                Enchantment ench = CardDB.getEnchantmentFromCardID(CardDB.Instance.cardIdstringToEnum(bhu.GetCardId()));
                ench.creator = bhu.GetCreatorId();
                ench.controllerOfCreator = bhu.GetControllerId();
                ench.cantBeDispelled = false;
                //if (bhu.c) ench.cantBeDispelled = true;

                foreach (Minion m in this.ownMinions)
                {
                    if (m.entitiyID == bhu.GetAttached())
                    {
                        m.enchantments.Add(ench);
                        //Helpfunctions.Instance.ErrorLog("add enchantment " +bhu.GetCardId()+" to: " + m.entitiyID);
                    }

                }

                foreach (Minion m in this.enemyMinions)
                {
                    if (m.entitiyID == bhu.GetAttached())
                    {
                        m.enchantments.Add(ench);
                    }

                }

            }
            */
        }

        private void getHandcards(HSRangerLib.BotBase rangerbot)
        {
            handCards.Clear();
            this.anzcards = 0;
            this.enemyAnzCards = 0;
            List<Entity> list = rangerbot.FriendHand;

            int elementalLastturn = 0;
            foreach (Entity item in list)
            {

                Entity entitiy = item;
           
                if (entitiy.ControllerId == this.ownPlayerController && entitiy.ZonePosition >= 1) // own handcard
                {
                    CardDB.Card c = CardDB.Instance.getCardDataFromID(CardDB.Instance.cardIdstringToEnum(entitiy.CardId));

                    //c.cost = entitiy.GetCost();
                    //c.entityID = entitiy.GetEntityId();

                    Handmanager.Handcard hc = new Handmanager.Handcard();
                    hc.card = c;
                    hc.position = entitiy.ZonePosition;
                    hc.entity = entitiy.EntityId;
                    hc.manacost = entitiy.Cost;
                    hc.addattack = 0;
                    //Helpfunctions.Instance.ErrorLog("hc "+ entitiy.ZonePosition + " ." + entitiy.CardId + ". " + entitiy.Cost + "  " + c.name);
                    int attackchange = entitiy.ATK - c.Attack;
                    int hpchange = entitiy.Health - c.Health;
                    hc.addattack = attackchange;
                    hc.addHp = hpchange;
                    hc.elemPoweredUp = entitiy.GetTagValue((int)GAME_TAG.ELEMENTAL_POWERED_UP);
                    if (hc.elemPoweredUp > 0) elementalLastturn = 1;
                    if (entitiy.HasTagValue((int)GAME_TAG.POWERED_UP)) hc.powerup = true;

                    handCards.Add(hc);
                    this.anzcards++;
                }


            }
            if (elementalLastturn > 0) Hrtprozis.Instance.updateElementals(elementalLastturn);

            Dictionary<int, Entity> allEntitys = new Dictionary<int, Entity>();

            foreach (var item in rangerbot.gameState.GameEntityList)
            {
                allEntitys.Add(item.EntityId, item);
            }


            foreach (Entity ent in allEntitys.Values)
            {
                if (ent.ControllerId != this.ownPlayerController && ent.ZonePosition >= 1 && ent.Zone == HSRangerLib.TAG_ZONE.HAND) // enemy handcard
                {
                    this.enemyAnzCards++;

                    //dont know if we can read this so ;D
                    if (CardDB.Instance.cardIdstringToEnum(ent.CardId) == CardDB.cardIDEnum.LOE_007t) this.enemyCursedCardsInHand++;
                }
            }

            //search for choice-cards in HR:
            this.choiceCards.Clear();
            this.choiceCardsEntitys.Clear();
            foreach (Entity ent in allEntitys.Values)
            {
                if (ent.ControllerId == this.ownPlayerController && ent.Zone == HSRangerLib.TAG_ZONE.SETASIDE) // choice cards are in zone setaside (but thats not all D:)
                {
                    if (ent.CardState == ActorStateType.CARD_SELECTABLE) //in HR these cards (setaside + card_selectable) are choice/tracking/discover-cards
                    {
                        this.choiceCards.Add(CardDB.Instance.cardIdstringToEnum(ent.CardId));
                        this.choiceCardsEntitys.Add(ent.EntityId);
                    }
                }
            }

        }





        private void getDecks(HSRangerLib.BotBase rangerbot)
        {
            Dictionary<int, Entity> allEntitys = new Dictionary<int, Entity>();

            // add json.net nuget package to use this debug code
            //string path = SilverFishBotPath.AssemblyDirectory + System.IO.Path.DirectorySeparatorChar + "HRERRORLogs" + System.IO.Path.DirectorySeparatorChar;
            //System.IO.Directory.CreateDirectory(path);
            //string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(rangerbot.gameState.GameEntityList, Newtonsoft.Json.Formatting.Indented);
            //System.IO.File.WriteAllText(path + "HRErrorLog" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".txt", jsonData);

            foreach (Entity item in rangerbot.gameState.GameEntityList)
            {
                allEntitys.Add(item.EntityId, item);
                if (item.Zone == HSRangerLib.TAG_ZONE.GRAVEYARD)
                {
                    Helpfunctions.Instance.logg("ent.Zone FOUND" + item.Zone + item.EntityId);
                    Helpfunctions.Instance.ErrorLog("ent.Zone FOUND" + item.Zone + item.EntityId);
                }
            }

            int owncontroler = rangerbot.gameState.LocalControllerId;
            int enemycontroler = rangerbot.gameState.RemoteControllerId;
            List<CardDB.cardIDEnum> ownCards = new List<CardDB.cardIDEnum>();
            List<CardDB.cardIDEnum> enemyCards = new List<CardDB.cardIDEnum>();
            List<GraveYardItem> graveYard = new List<GraveYardItem>();

            foreach (Entity ent in allEntitys.Values)
            {
                //Helpfunctions.Instance.logg("Zone=" + ent.Zone + " id=" + ent.EntityId + ent.CardState);
                //Helpfunctions.Instance.ErrorLog("Zone=" + ent.Zone + " id=" + ent.EntityId  + ent.CardState );
                if (ent.Zone == HSRangerLib.TAG_ZONE.SECRET && ent.ControllerId == enemycontroler) continue; // cant know enemy secrets :D
                if (ent.Zone == HSRangerLib.TAG_ZONE.DECK) continue;
                CardDB.cardIDEnum cardid = CardDB.Instance.cardIdstringToEnum(ent.CardId);

                //string owner = "own";
                //if (ent.GetControllerId() == enemycontroler) owner = "enemy";
                //if (ent.GetControllerId() == enemycontroler && ent.GetZone() == HRCardZone.HAND) Helpfunctions.Instance.logg("enemy card in hand: " + "cardindeck: " + cardid + " " + ent.GetName());
                //if (cardid != CardDB.cardIDEnum.None) Helpfunctions.Instance.logg("cardindeck: " + cardid + " " + ent.GetName() + " " + ent.GetZone() + " " + owner + " " + ent.GetCardType());
                if (cardid != CardDB.cardIDEnum.None)
                {
                    if (ent.Zone == HSRangerLib.TAG_ZONE.GRAVEYARD)
                    {
                        GraveYardItem gyi = new GraveYardItem(cardid, ent.EntityId, ent.ControllerId == owncontroler);
                        graveYard.Add(gyi);
                    }

                    int creator = ent.CreatorId;
                    if (creator != 0 && creator != owncontroler && creator != enemycontroler) continue; //if creator is someone else, it was not played

                    if (ent.ControllerId == owncontroler) //or controler?
                    {
                        if (ent.Zone == HSRangerLib.TAG_ZONE.GRAVEYARD)
                        {
                            ownCards.Add(cardid);
                            if (cardid == CardDB.cardIDEnum.UNG_067t1) OwnCrystalCore = 5;
                            if (cardid == CardDB.cardIDEnum.UNG_116t) ownMinionsCost0 = true;
                        }

                        if (ent.Zone == HSRangerLib.TAG_ZONE.PLAY)
                        {
                            if (cardid == CardDB.cardIDEnum.UNG_067t1) OwnCrystalCore = 5;
                            if (cardid == CardDB.cardIDEnum.UNG_116t) ownMinionsCost0 = true;
                        }

                    }
                    else
                    {
                        if (ent.Zone == HSRangerLib.TAG_ZONE.GRAVEYARD)
                        {
                            enemyCards.Add(cardid);
                            if (cardid == CardDB.cardIDEnum.UNG_067t1) EnemyCrystalCore = 5;
                        }

                        if (ent.Zone == HSRangerLib.TAG_ZONE.PLAY)
                        {
                            if (cardid == CardDB.cardIDEnum.UNG_067t1)
                            {
                                EnemyCrystalCore = 5;
                                Helpfunctions.Instance.logg("ENEMYCRYSTALCOREFOUND");
                                Helpfunctions.Instance.ErrorLog("ENEMYCRYSTALCOREFOUND");
                            }
                        }

                    }
                }
            }

            Probabilitymaker.Instance.setOwnCards(ownCards);
            Probabilitymaker.Instance.setEnemyCards(enemyCards);
            bool isTurnStart = false;
            if (Ai.Instance.nextMoveGuess.mana == -100)
            {
                isTurnStart = true;
                Ai.Instance.updateTwoTurnSim();
            }
            Probabilitymaker.Instance.setGraveYard(graveYard, isTurnStart);

        }

        private void updateBehaveString(Behavior botbase)
        {
            this.botbehave = "rush";
            if (botbase is BehaviorFace) this.botbehave = "face";
            if (botbase is BehaviorControl) this.botbehave = "control";
            if (botbase is BehaviorMana) this.botbehave = "mana";
            if (botbase is BehaviorAggroWarlock) this.botbehave = "aggrowarlock";
            if (botbase is BehaviorAggroshaman) this.botbehave = "aggroshaman";
            this.botbehave += " " + Ai.Instance.maxwide;
            this.botbehave += " face " + ComboBreaker.Instance.attackFaceHP;
            if (Settings.Instance.secondTurnAmount > 0)
            {
                if (Ai.Instance.nextMoveGuess.mana == -100)
                {
                    Ai.Instance.updateTwoTurnSim();
                }
                this.botbehave += " twoturnsim " + Settings.Instance.secondTurnAmount + " ntss " + Settings.Instance.nextTurnDeep + " " + Settings.Instance.nextTurnMaxWide + " " + Settings.Instance.nextTurnTotalBoards;
            }

            if (Settings.Instance.playarround)
            {
                this.botbehave += " playaround";
                this.botbehave += " " + Settings.Instance.playaroundprob + " " + Settings.Instance.playaroundprob2;
            }

            this.botbehave += " ets " + Settings.Instance.enemyTurnMaxWide;

            if (Settings.Instance.simEnemySecondTurn)
            {
                this.botbehave += " ets2 " + Settings.Instance.enemyTurnMaxWideSecondTime;
                this.botbehave += " ents " + Settings.Instance.enemySecondTurnMaxWide;
            }

            if (Settings.Instance.useSecretsPlayArround)
            {
                this.botbehave += " secret";
            }

            if (Settings.Instance.secondweight != 0.5f)
            {
                this.botbehave += " weight " + (int)(Settings.Instance.secondweight * 100f);
            }

            if (Settings.Instance.simulatePlacement)
            {
                this.botbehave += " plcmnt";
            }


        }

        public static int getLastAffected(int entityid)
        {

            if (latestGameState != null)
            {
                foreach (var item in latestGameState.GameEntityList)
                {
                    if (item.LastAffectedById == entityid)
                    {
                        return item.EntityId;
                    }
                }
            }

            return 0;
        }

        public static int getCardTarget(int entityid)
        {

            if (latestGameState != null)
            {
                foreach (var item in latestGameState.GameEntityList)
                {
                    if (item.EntityId == entityid)
                    {
                        return item.CardTargetId;
                    }
                }
            }


            return 0;
        }

        //public void testExternal()
        //{
        //    BoardTester bt = new BoardTester("");
        //    this.currentMana = Hrtprozis.Instance.currentMana;
        //    this.ownMaxMana = Hrtprozis.Instance.ownMaxMana;
        //    this.enemyMaxMana = Hrtprozis.Instance.enemyMaxMana;
        //    printstuff(true);
        //    readActionFile();
        //}

        private void printstuff(Playfield p, bool runEx)
        {
            string dtimes = DateTime.Now.ToString("HH:mm:ss:ffff");
            String completeBoardString = p.getCompleteBoardForSimulating(this.botbehave, this.versionnumber, dtimes);
            
            Helpfunctions.Instance.logg(completeBoardString);

            if (runEx)
            {
                Ai.Instance.currentCalculatedBoard = dtimes;
                Helpfunctions.Instance.resetBuffer();
                if (!Settings.Instance.useNetwork)
                {
                    Helpfunctions.Instance.writeBufferToActionFile();
                    Helpfunctions.Instance.resetBuffer();
                }

                Helpfunctions.Instance.writeToBuffer(completeBoardString);
                Helpfunctions.Instance.writeBufferToFile();
            }

        }

        public bool readActionFile(bool passiveWaiting = false)
        {
            bool readed = true;
            List<string> alist = new List<string>();
            float value = 0f;
            string boardnumm = "-1";
            this.waitingForSilver = true;
            int trackingchoice = 0;
            int trackingstate = 0;
            bool network = Settings.Instance.useNetwork;

            while (readed)
            {
                try
                {
                    string data = "";
                    System.Threading.Thread.Sleep(5);
                    if (network)
                    {
                        KeyValuePair<string, string> msg = FishNet.Instance.readMessage();
                        if (msg.Key != "actionstodo.txt")
                        {
                            Helpfunctions.Instance.ErrorLog("[Program] Ignoring Message: " + msg.Key);
                            continue;
                        }
                        Helpfunctions.Instance.ErrorLog("[Program] Message Type: " + msg.Key);
                        data = msg.Value;
                    }
                    else
                    {
                        data = System.IO.File.ReadAllText(Settings.Instance.path + "actionstodo.txt");
                    }
                    //if (data == "") Helpfunctions.Instance.ErrorLog($"[Program] Message Data: empty");
                    //if (data == "<EoF>" && data.EndsWith("<EoF>")) Helpfunctions.Instance.ErrorLog($"[Program] Message Data: <EoF>");
                    //if (!data.EndsWith("<EoF>")) Helpfunctions.Instance.ErrorLog($"[Program] Message Data: missing <EoF>");

                    if (data != "" && data != "<EoF>" && data.EndsWith("<EoF>"))
                    {
                        //Helpfunctions.Instance.ErrorLog($"[Program] Message Data:\r\n{data}");
                        data = data.Replace("<EoF>", "");
                        //Helpfunctions.Instance.ErrorLog(data);
                        if (!network)
                        {
                            Helpfunctions.Instance.resetBuffer();
                            Helpfunctions.Instance.writeBufferToActionFile();
                        }
                        alist.AddRange(data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                        string board = alist[0];
                        if (board.StartsWith("board "))
                        {
                            boardnumm = (board.Split(' ')[1].Split(' ')[0]);
                            alist.RemoveAt(0);
                            if (boardnumm != Ai.Instance.currentCalculatedBoard)
                            {
                                if (passiveWaiting)
                                {
                                    System.Threading.Thread.Sleep(50);
                                    return false;
                                }
                                continue;
                            }
                        }
                        string first = alist[0];
                        if (first.StartsWith("value "))
                        {
                            value = float.Parse((first.Split(' ')[1].Split(' ')[0]));
                            alist.RemoveAt(0);
                        }

                        first = alist[0];

                        if (first.StartsWith("discover "))
                        {
                            string trackingstuff = first.Replace("discover ", "");
                            trackingchoice = Convert.ToInt32(trackingstuff.Split(',')[0]);
                            trackingstate = Convert.ToInt32(trackingstuff.Split(',')[1]);
                            alist.RemoveAt(0);
                        }
                        readed = false;
                    }
                    else
                    {
                        if (passiveWaiting)
                        {
                            return false;
                        }
                    }

                }
                catch
                {
                    System.Threading.Thread.Sleep(5);
                }
            }
            this.waitingForSilver = false;
            Helpfunctions.Instance.logg("received " + boardnumm + " actions to do: (currtime = " + DateTime.Now.ToString("HH:mm:ss.ffff") + ")");
            Ai.Instance.currentCalculatedBoard = "0";
            Playfield p = new Playfield();
            List<Action> aclist = new List<Action>();

            foreach (string a in alist)
            {
                aclist.Add(new Action(a, p));
                Helpfunctions.Instance.logg(a);
            }

            Ai.Instance.setBestMoves(aclist, value, trackingchoice, trackingstate);

            return true;
        }


    }

    public sealed class Helpfunctions
    {
        private static Helpfunctions instance;

        public static Helpfunctions Instance
        {
            get
            {
                return instance ?? (instance = new Helpfunctions());
            }
        }

        private Helpfunctions()
        {
            //System.IO.File.WriteAllText(Settings.Instance.logpath + Settings.Instance.logfile, "");
        }

        private bool writelogg = true;
        public void loggonoff(bool onoff)
        {
            //writelogg = onoff;
        }

        private bool filecreated = false;
        public void createNewLoggfile()
        {
            filecreated = false;
        }

        private List<string> loggBuffer = new List<string>(Settings.Instance.logBuffer + 1);
        public void logg(string s)
        {
            loggBuffer.Add(s);

            if (loggBuffer.Count > Settings.Instance.logBuffer) flushLogg();
        }

        public void flushLogg()
        {
            if (loggBuffer.Count == 0) return;
            try
            {
                File.AppendAllLines(Settings.Instance.logpath + Settings.Instance.logfile, loggBuffer);
                loggBuffer.Clear();
            }
            catch
            {

            }
        }

        public DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private List<string> errorLogBuffer = new List<string>(Settings.Instance.logBuffer + 1);
        public void ErrorLog(string s)
        {
            if (!writelogg) return;
            errorLogBuffer.Add(DateTime.Now.ToString("HH:mm:ss: ") + s);

            if (errorLogBuffer.Count > Settings.Instance.logBuffer) flushErrorLog();
        }

        public void flushErrorLog()
        {
            if (errorLogBuffer.Count == 0) return;
            try
            {
                File.AppendAllLines(Settings.Instance.logpath + "Logging.txt", errorLogBuffer);
                errorLogBuffer.Clear();
            }
            catch
            {

            }
        }

        public Task startFlushingLogBuffers(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Instance.flushLogBuffersAsync(cancellationToken), cancellationToken);
        }

        public async Task flushLogBuffersAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Instance.flushLogg();
                Instance.flushErrorLog();
                await Task.Delay(50, cancellationToken);
            }
        }


        string sendbuffer = "";
        public void resetBuffer()
        {
            this.sendbuffer = "";
        }

        public void writeToBuffer(string data)
        {
            this.sendbuffer += data + "\r\n";
        }

        public void writeBufferToNetwork(string msgtype)
        {
            FishNet.Instance.sendMessage(msgtype + "\r\n" + this.sendbuffer);
        }

        public void writeBufferToFile()
        {
            bool writed = true;
            this.sendbuffer += "<EoF>";
            //this.ErrorLog("write to crrntbrd file: " + sendbuffer);
            while (writed)
            {
                try
                {
                    if (Settings.Instance.useNetwork) writeBufferToNetwork("crrntbrd.txt");
                    else System.IO.File.WriteAllText(Settings.Instance.path + "crrntbrd.txt", this.sendbuffer);
                    writed = false;
                }
                catch
                {
                    writed = true;
                }
            }
            this.sendbuffer = "";
        }

        public void writeBufferToDeckFile()
        {
            bool writed = true;
            this.sendbuffer += "<EoF>";
            while (writed)
            {
                try
                {
                    if (Settings.Instance.useNetwork) writeBufferToNetwork("curdeck.txt");
                    else System.IO.File.WriteAllText(Settings.Instance.path + "curdeck.txt", this.sendbuffer);
                    writed = false;
                }
                catch
                {
                    writed = true;
                }
            }
            this.sendbuffer = "";
        }

        public void writeBufferToActionFile()
        {
            bool writed = true;
            this.sendbuffer += "<EoF>";
            //this.ErrorLog("write to action file: "+ sendbuffer);
            while (writed)
            {
                try
                {
                    if (Settings.Instance.useNetwork) writeBufferToNetwork("actionstodo.txt");
                    else System.IO.File.WriteAllText(Settings.Instance.path + "actionstodo.txt", this.sendbuffer);
                    writed = false;
                }
                catch
                {
                    writed = true;
                }
            }
            this.sendbuffer = "";
        }

        public void writeBufferToCardDB()
        {
            bool writed = true;
            while (writed)
            {
                try
                {
                    System.IO.File.WriteAllText(Settings.Instance.path + "newCardDB.cs", this.sendbuffer);
                    writed = false;
                }
                catch
                {
                    writed = true;
                }
            }
            this.sendbuffer = "";
        }
    }


    // the ai :D
    //please ask/write me if you use this in your project

}
