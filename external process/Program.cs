using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
    }

    class Program
    {
        static void Main(string[] args)
        { 
            Bot b = new Bot();
            bool readed = false;
            while(!readed)
            {
                try
                {
                    string data = System.IO.File.ReadAllText("crrntbrd.txt");
                    //Helpfunctions.Instance.ErrorLog(data);
                    if (data != "" && data != "<EoF>")
                    {
                        data = data.Replace("<EoF>", "");
                        Helpfunctions.Instance.resetBuffer();
                        Helpfunctions.Instance.writeBufferToFile();
                        //readed = true;
                        b.doData(data);
                    }
                }
                catch
                {

                }
                try
                {
                    string data = System.IO.File.ReadAllText("curdeck.txt");
                    //Helpfunctions.Instance.ErrorLog(data);
                    if (data != "" && data != "<EoF>")
                    {
                        data = data.Replace("<EoF>", "");
                        Helpfunctions.Instance.resetBuffer();
                        Helpfunctions.Instance.writeBufferToDeckFile();
                        //readed = true;
                        b.doDeckData(data);
                    }
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(10);
            }

            Console.ReadLine();
        }
    }

    public class Bot
    {
        PenalityManager penman = PenalityManager.Instance;
        DateTime starttime = DateTime.Now;
        Silverfish sf;
        Behavior behave = new BehaviorControl();

        public Bot()
        {
            starttime = DateTime.Now;
            Settings set = Settings.Instance;
            this.sf = Silverfish.Instance;
            set.setSettings();
            sf.setnewLoggFile();

            bool teststuff = true;
            bool printstuff = true;

            if (teststuff)
            {
                Helpfunctions.Instance.logg("teststuff");
                Playfield p = new Playfield();
                Ai.Instance.autoTester(printstuff);
            }
            Helpfunctions.Instance.ErrorLog("wait for board...");
        }

        public void doData(string data)
        {
            Ai.Instance.updateTwoTurnSim();
            Ai.Instance.autoTester(false, data);
            Helpfunctions.Instance.resetBuffer();
            Helpfunctions.Instance.writeToBuffer("board " + Ai.Instance.currentCalculatedBoard);
            Helpfunctions.Instance.writeToBuffer("value " + Ai.Instance.bestmoveValue);
            Helpfunctions.Instance.writeToBuffer("discover " + Ai.Instance.bestTracking + "," + Ai.Instance.bestTrackingStatus);

            if (Ai.Instance.bestmove != null)
            {
                Ai.Instance.bestmove.print(true);
                foreach (Action a in Ai.Instance.bestActions)
                {
                    a.print(true);
                }
            }
            Helpfunctions.Instance.writeBufferToActionFile();
            Ai.Instance.currentCalculatedBoard = "1";
            Helpfunctions.Instance.ErrorLog("wait for next board...");

            //sf.readActionFile();
        }

        public void doDeckData(string data)
        {
            data = data.Trim();
            //Helpfunctions.Instance.ErrorLog(data);
            string deckname = data.Split(';')[0];
            string ownname = data.Split(';')[1];
            string enemyname = data.Split(';')[2];
            HeroEnum heroname = Hrtprozis.Instance.heroNametoEnum(ownname);
            HeroEnum enemyHeroname = Hrtprozis.Instance.heroNametoEnum(enemyname);

            if (Hrtprozis.Instance.deckName != deckname || heroname != Hrtprozis.Instance.heroname || enemyHeroname != Hrtprozis.Instance.enemyHeroname)
            {
                Hrtprozis.Instance.setDeckName(deckname);
                Hrtprozis.Instance.setHeroName(ownname);
                Hrtprozis.Instance.setEnemyHeroName(enemyname);
                ComboBreaker.Instance.updateInstance();
                Discovery.Instance.updateInstance();
                Mulligan.Instance.updateInstance();
                Settings.Instance.updateInstance();
            }
        }

        public void testing(int start)
        {
            for (int i = start; i < 900; i++)
            {
                Handmanager.Instance.anzcards = 1;
                Handmanager.Instance.handCards.Clear();
                Handmanager.Handcard hc = new Handmanager.Handcard();
                hc.manacost = 1;
                hc.position = 1;
                hc.entity = 122;
                hc.card = CardDB.Instance.getCardDataFromID((CardDB.cardIDEnum)i);
                Handmanager.Instance.handCards.Add(hc);
                Helpfunctions.Instance.ErrorLog("test " + i + " " + hc.card.name + " " + hc.card.cardIDenum);
                if (hc.card.sim_card == null)
                {
                    Helpfunctions.Instance.logg("cant test " + i + " " + hc.card.name);
                }
                else
                {
                    Ai.Instance.dosomethingclever(this.behave);
                }
            }
        }

    }

    public sealed class Silverfish
    {
        public string versionnumber = "122.5SE";
        private bool singleLog = false;

        Settings sttngs = Settings.Instance;

        List<Minion> ownMinions = new List<Minion>();
        List<Minion> enemyMinions = new List<Minion>();
        List<Handmanager.Handcard> handCards = new List<Handmanager.Handcard>();
        List<string> ownSecretList = new List<string>();

        CardDB.Card heroAbility = new CardDB.Card();
        CardDB.Card enemyAbility = new CardDB.Card();
        
        private int anzOgOwnCThunHpBonus = 0;
        private int anzOgOwnCThunAngrBonus = 0;
        private int anzOgOwnCThunTaunt = 0;

        private static readonly Silverfish instance = new Silverfish();

        static Silverfish() { } // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit

        public static Silverfish Instance
        {
            get
            {
                return instance;
            }
        }

        private Silverfish()
        {
            this.singleLog = Settings.Instance.writeToSingleFile;
            Helpfunctions.Instance.ErrorLog("init Silverfish V" + versionnumber);
            string path = "";
            //System.IO.Directory.CreateDirectory(path);
            sttngs.setFilePath(SilverFishBotPath.AssemblyDirectory);

            if (!singleLog)
            {
                sttngs.setLoggPath(path);
            }
            else
            {
                sttngs.setLoggPath("");
                sttngs.setLoggFile("UILogg.txt");
                try
                {
                    Helpfunctions.Instance.createNewLoggfile();
                }
                    catch
                {
                    }
            }
            PenalityManager.Instance.setCombos();
            Mulligan.Instance.runDebugTest();
            Discovery d = Discovery.Instance; // read the discover list
        }

        public void setnewLoggFile()
        {
            if (!singleLog)
            {
                sttngs.setLoggFile("UILogg" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".txt");
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

        public void readActionFile(bool passiveWaiting = false)
        {
            Ai.Instance.nextMoveGuess = new Playfield();
            bool readed = true;
            List<string> alist = new List<string>();
            float value = 0f;
            string boardnumm = "-1";
            int trackingchoice = 0;
            int trackingstate = 0;
            while (readed)
            {
                try
                {
                    string data = System.IO.File.ReadAllText(Settings.Instance.path + "actionstodo.txt");
                    if (data != "" && data != "<EoF>" && data.EndsWith("<EoF>"))
                    {
                        data = data.Replace("<EoF>", "");
                        //Helpfunctions.Instance.ErrorLog(data);
                        Helpfunctions.Instance.resetBuffer();
                        Helpfunctions.Instance.writeBufferToActionFile();
                        alist.AddRange(data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                        string board = alist[0];
                        if (board.StartsWith("board "))
                        {
                            boardnumm = (board.Split(' ')[1].Split(' ')[0]);
                            alist.RemoveAt(0);
                            /*if (boardnumm != Ai.Instance.currentCalculatedBoard)
                            {
                                if (passiveWaiting)
                                {
                                    System.Threading.Thread.Sleep(10);
                                    return;
                                }
                                continue;
                            }*/
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
                            trackingchoice = Convert.ToInt32(first.Split(',')[0]);
                            trackingstate = Convert.ToInt32(first.Split(',')[1]);
                            alist.RemoveAt(0);
                        }

                        readed = false;
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);
                        if (passiveWaiting)
                        {
                            return;
                        }
                    }

                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }

            }
            Helpfunctions.Instance.logg("received " + boardnumm + " actions to do:");
            Ai.Instance.currentCalculatedBoard = "0";
            Playfield p = new Playfield();
            List<Action> aclist = new List<Action>();

            foreach (string a in alist)
            {
                aclist.Add(new Action(a, p));
                Helpfunctions.Instance.logg(a);
            }

            Ai.Instance.setBestMoves(aclist, value, trackingchoice, trackingstate);

        }

        public static int getLastAffected(int entityid)
        {
            return 0;
        }

        public static int getCardTarget(int entityid)
        {
            return 0;
        }


    }

    public sealed class Helpfunctions
    {
        private static readonly Helpfunctions instance = new Helpfunctions();

        static Helpfunctions() { } // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit

        public static Helpfunctions Instance
        {
            get
            {
                return instance;
            }
        }

        private Helpfunctions()
        {
            //System.IO.File.WriteAllText(Settings.Instance.logpath + Settings.Instance.logfile, "");
        }
        
        public void loggonoff(bool onoff)
        {
            //writelogg = onoff;
        }

        public void createNewLoggfile()
        {
            //System.IO.File.WriteAllText(Settings.Instance.logpath + Settings.Instance.logfile, "");
        }

        public void logg(string s)
        {


            /*if (!writelogg) return;
            try
            {
                using (StreamWriter sw = File.AppendText(Settings.Instance.logpath + Settings.Instance.logfile))
                {
                    sw.WriteLine(s);
                }
            }
            catch { }*/
            Console.WriteLine(s);
        }

        public DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public void ErrorLog(string s)
        {
            //HREngine.API.Utilities.HRLog.Write(s);
            Console.WriteLine(s);
        }

        string sendbuffer = "";
        public void resetBuffer()
        {
            this.sendbuffer = "";
        }

        public void writeToBuffer(string data)
        {
            this.sendbuffer += "\r\n" + data;
        }

        public void writeBufferToFile()
        {
            bool writed = true;
            this.sendbuffer += "<EoF>";
            while (writed)
            {
                try
                {
                    System.IO.File.WriteAllText(Settings.Instance.path + "crrntbrd.txt", this.sendbuffer);
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
                    System.IO.File.WriteAllText(Settings.Instance.path + "curdeck.txt", this.sendbuffer);
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
            this.ErrorLog("write to action file: "+ sendbuffer);
            while (writed)
            {
                try
                {
                    System.IO.File.WriteAllText(Settings.Instance.path + "actionstodo.txt", this.sendbuffer);
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
