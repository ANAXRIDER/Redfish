using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HREngine.Bots
{
    class Class1
    {
        public static void Main()
        {


            Ai daum = Ai.Instance;

            if (daum.bestmove != null)// || daum.bestmove.actionType != actionEnum.endturn || daum.bestmove.actionType != actionEnum.useHeroPower)
            //if (daum.bestActions[0].actionType != actionEnum.endturn  && daum.bestActions.Count >= 1 && Playfield.Instance.turnCounter == 0)
            {
                if (daum.bestmove.actionType != actionEnum.endturn || daum.bestmove.actionType != actionEnum.useHeroPower) Helpfunctions.Instance.logg("daum.bestmove.card.card.name  " + daum.bestmove.card.card.name);
                Helpfunctions.Instance.logg("daum.bestmove.card.card.name  ");
                Helpfunctions.Instance.logg("daum.bestmove.card.card.name  ");
                switch (daum.bestmove.card.card.name)
                {
                    case CardDB.cardName.abusivesergeant:
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        System.Threading.Thread.Sleep(2200);
                        break;
                    case CardDB.cardName.darkirondwarf:
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        System.Threading.Thread.Sleep(2200);
                        break;
                    case CardDB.cardName.defenderofargus:
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.logg("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        Helpfunctions.Instance.ErrorLog("battlecry detected sleep 900ms");
                        System.Threading.Thread.Sleep(2200);
                        break;
                    default:
                        break;

                }
            }
        }
    }
}
