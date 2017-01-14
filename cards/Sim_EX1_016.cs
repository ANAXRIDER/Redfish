using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_EX1_016 : SimTemplate //sylvanaswindrunner
	{
        //todo make it better
//    todesröcheln:/ übernehmt die kontrolle über einen zufälligen feindlichen diener.
        public override void onDeathrattle(Playfield p, Minion m)
        {
            List<Minion> tmp = (m.own) ? p.enemyMinions : p.ownMinions;
            if (tmp.Count >= 1)
            {
                Minion target = null;
                int value = 10000;
                bool found = false;

                //search smallest minion:
                if (m.own)
                {
                    foreach (Minion mnn in tmp)
                    {
                        if (mnn.Hp < value && mnn.Hp >= 1)
                        {
                            target = mnn;
                            value = target.Hp;
                            found = true;
                        }
                    }
                }
                else
                {
                    //steal a minion with has not attacked or has taunt
                    value = -1000;

                    List<Minion> temp = new List<Minion>(p.ownMinions);
                    temp.Sort((a, b) => (a.Angr * 2 + a.Hp * 1).CompareTo((b.Angr * 2 + b.Hp * 1))); // increasing value

                    //foreach (Minion mnnn in temp)
                    //{
                    //    Helpfunctions.Instance.ErrorLog("temp  " + mnnn.name);
                    //}


                    if (p.ownMinions.Count >= 1)
                    {
                        target = temp[Math.Max(p.ownMinions.Count / 2 ,0)];
                        found = true;
                    }

                    foreach (Minion mnn in tmp)
                    {
                        //int special = 0;
                        ////int special = (m.Ready) ? mnn.Angr : 0;
                        ////special += (m.taunt) ? 5 : 0;
                        //special += mnn.handcard.card.cost;
                        //if (special > value)
                        //{
                        //    target = mnn;
                        //    value = special;
                        //    found = true;
                        //}                        
                        if (target.destroyOnOwnTurnStart || target.destroyOnOwnTurnEnd || target.destroyOnEnemyTurnStart || target.destroyOnEnemyTurnEnd) target = temp[Math.Max (p.ownMinions.Count - 2, 0)];                       
                        if (mnn.playedThisTurn && mnn.Angr >= p.searchRandomMinion(tmp, Playfield.searchmode.searchHighestAttack).Angr) target = p.searchRandomMinion(tmp, Playfield.searchmode.searchHighestAttack);
                        if (mnn.Ready) target = p.searchRandomMinion(tmp, Playfield.searchmode.searchHighestAttack);
                        if (mnn.taunt) target = mnn;
                    }
                }
                if (found) p.minionGetControlled(target, m.own, false);
                //if (found) Helpfunctions.Instance.ErrorLog("found  " + target.name);
            }
        }
	}
}