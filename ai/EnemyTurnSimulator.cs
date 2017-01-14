namespace HREngine.Bots
{
    using System;
    using System.Collections.Generic;

    public class EnemyTurnSimulator
    {
        public bool Aggrodeck;

        public int thread = 0;

        private List<Playfield> posmoves = new List<Playfield>(500); //initializing 500 should be plenty even for extreme settings

        public int maxwide = 20;

        public Behavior botBase;

        private Movegenerator movegen = Movegenerator.Instance;

        private PenalityManager penmanager = PenalityManager.Instance;

        public void setMaxwideFirstStep(bool firstTurn)
        {
            maxwide = Settings.Instance.enemyTurnMaxWide;
            if (!firstTurn) maxwide = Settings.Instance.enemyTurnMaxWide;
        }

        public void setMaxwideSecondStep(bool firstTurn)
        {
            maxwide = Settings.Instance.enemyTurnMaxWideSecondTime;
            if (!firstTurn) maxwide = Settings.Instance.enemyTurnMaxWide;
        }

        public void simulateEnemysTurn(Playfield rootfield, bool simulateTwoTurns, bool playaround, bool print, int pprob, int pprob2)
        {
            if (botBase == null) botBase = Ai.Instance.botBase;

            bool havedonesomething = true;
            posmoves.Clear();
            if (print)
            {
                Helpfunctions.Instance.ErrorLog("board at enemyturn start-----------------------------");
                rootfield.value = botBase.getPlayfieldValue(rootfield);
                rootfield.printBoard();
            }
            posmoves.Add(new Playfield(rootfield));
            //posmoves[0].prepareNextTurn(false);
            List<Playfield> temp = new List<Playfield>();
            int deep = 0;
            int enemMana = rootfield.enemyMaxMana;

            //get rid of cursed! ?
            if (posmoves[0].anzEnemyCursed >= 1)
            {
                int curseds = posmoves[0].anzEnemyCursed;

                for (int ii = curseds; ii > 0; ii--)
                {
                    if (enemMana >= 2)
                    {
                        enemMana -= 2;
                        posmoves[0].anzEnemyCursed--;
                    }
                }
            }

            if (print)
            { Console.WriteLine("enemMana "+ enemMana); }

            //playing aoe-effects if activated (and we didnt play loatheb)
            if (playaround && rootfield.anzOwnLoatheb == 0)
            {
                float oldval = botBase.getPlayfieldValueEnemy(posmoves[0]);
                posmoves[0].value = int.MinValue;
                enemMana = posmoves[0].EnemyCardPlaying(rootfield.enemyHeroName, enemMana, rootfield.enemyAnzCards, pprob, pprob2);
                float newval = botBase.getPlayfieldValueEnemy(posmoves[0]);
                posmoves[0].value = int.MinValue;
                posmoves[0].enemyAnzCards--;
                posmoves[0].triggerCardsChanged(false);
                posmoves[0].mana = enemMana;
                if (oldval < newval)
                {
                    posmoves.Clear();
                    posmoves.Add(new Playfield(rootfield));
                }
            }



            //play ability!

            if (posmoves[0].enemyAbilityReady && enemMana >= 2 && posmoves[0].enemyHeroAblility.card.canplayCard(posmoves[0], 0) && rootfield.anzOwnSaboteur == 0)
            {
                int abilityPenality = 0;

                havedonesomething = true;
                // if we have mage or priest or hunter, we have to target something####################################################


                if (penmanager.TargetAbilitysDatabase.ContainsKey(posmoves[0].enemyHeroAblility.card.cardIDenum))
                {

                    List<Minion> trgts = posmoves[0].enemyHeroAblility.card.getTargetsForCard(posmoves[0], false, false);
                    foreach (Minion trgt in trgts)
                    {
                        if (trgt.isHero) continue;//do play his ability in basics
                        Action a = new Action(actionEnum.useHeroPower, posmoves[0].enemyHeroAblility, null, 0, trgt, abilityPenality, 0);
                        Playfield pf = new Playfield(posmoves[0]);
                        pf.doAction(a);
                        posmoves.Add(pf);
                    }
                }
                else
                {
                    bool hasinspire = false;
                    foreach (Minion minie in rootfield.enemyMinions)
                    {
                        if (minie.handcard.card.Inspire) hasinspire = true;
                    }
                    // the other classes dont have to target####################################################
                    if ((rootfield.enemyHeroName == HeroEnum.thief && rootfield.enemyWeaponDurability == 0) || rootfield.enemyHeroName != HeroEnum.thief || hasinspire)
                    {
                        Action a = new Action(actionEnum.useHeroPower, posmoves[0].enemyHeroAblility, null, 0, null, abilityPenality, 0);
                        Playfield pf = new Playfield(posmoves[0]);
                        pf.doAction(a);
                        posmoves.Add(pf);
                    }
                }

            }


            foreach (Minion m in posmoves[0].enemyMinions)
            {
                if (m.Angr == 0) continue;
                m.numAttacksThisTurn = 0;
                m.playedThisTurn = false;
                m.updateReadyness();
            }


            int boardcount = 0;
            //movegen...

            int i = 0;
            int count = 0;
            Playfield p = null;
            Playfield bestold = null;

            while (havedonesomething)
            {

                temp.Clear();
                temp.AddRange(posmoves);
                havedonesomething = false;
                float bestoldval = int.MaxValue;

                //foreach (Playfield p in temp)
                count = temp.Count;
                for (i = 0; i < count; i++)
                {
                    p = temp[i];
                    if (p.complete)
                    {
                        continue;
                    }

                    List<Action> actions = movegen.getEnemyMoveList(p, false, true, true, 1);// 1 for not using ability moves

                    foreach (Action a in actions)
                    {
                        havedonesomething = true;
                        Playfield pf = new Playfield(p);
                        pf.doAction(a);
                        posmoves.Add(pf);

                        /*if (print)
                        {
                            a.print();
                        }*/
                        boardcount++;
                    }

                    p.endEnemyTurn();
                    //p.guessingHeroHP = rootfield.guessingHeroHP;
                    if (botBase.getPlayfieldValueEnemy(p) < bestoldval) // want the best enemy-play-> worst for us
                    {
                        bestoldval = botBase.getPlayfieldValueEnemy(p);
                        bestold = p;
                    }
                    posmoves.Remove(p);

                    if (boardcount >= maxwide) break;
                }

                if (bestoldval <= 10000 && bestold != null)
                {
                    posmoves.Add(bestold);
                }

                cuttingPosibilitiesET();

                deep++;
                if (boardcount >= maxwide) break;
            }

            //foreach (Playfield p in posmoves)
            count = posmoves.Count;
            for (i = 0; i < count; i++)
            {

                if (!posmoves[i].complete) posmoves[i].endEnemyTurn();
            }

            float bestval = int.MaxValue;
            Playfield bestplay = rootfield;// posmoves[0];

            //foreach (Playfield p in posmoves)
            count = posmoves.Count;
            for (i = 0; i < count; i++)
            {
                p = posmoves[i];
                //p.guessingHeroHP = rootfield.guessingHeroHP;
                float val = botBase.getPlayfieldValueEnemy(p);
                if (bestval > val)// we search the worst value
                {
                    bestplay = p;
                    bestval = val;
                }
                /*if (print)
                {
                    Helpfunctions.Instance.ErrorLog(""+val);
                    p.printBoard();
                }*/
            }
            if (print)
            {
                Helpfunctions.Instance.ErrorLog("best enemy board----------------------------------");
                bestplay.printBoard();
            }
            rootfield.value = bestplay.value;
            if (simulateTwoTurns && bestplay.ownHero.Hp > 0 && bestplay.value > -1000)
            {
                bestplay.prepareNextTurn(true);
                rootfield.value = Settings.Instance.firstweight * bestval + Settings.Instance.secondweight * Ai.Instance.nextTurnSimulator[this.thread].doallmoves(bestplay, false, print);
            }
        }

        public void cuttingPosibilitiesET()
        {
            Dictionary<Int64, Playfield> tempDict = new Dictionary<Int64, Playfield>(); //todo sepefeets - consider whether playfield.GetHashCode() should be int64 or if this should be int
            Playfield p = null;
            int max = posmoves.Count;
            for (int i = 0; i < max; i++)
            {
                p = posmoves[i];
                Int64 hash = p.GetHashCode();
                if (!tempDict.ContainsKey(hash)) tempDict.Add(hash, p);

            }
            posmoves.Clear();
            foreach (KeyValuePair<Int64, Playfield> d in tempDict)
            {
                posmoves.Add(d.Value);
            }
            tempDict.Clear();

            //Helpfunctions.Instance.logg(max + " enemy boards cut to " + this.posmoves.Count); //lots of spam just for debugging
        }

        CardDB.Card flame = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_614t);
        //CardDB.Card warsong = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.EX1_084);// RIP little friend
        CardDB.Card warriorweapon = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.CS2_106);

        private void doSomeBasicEnemyAi(Playfield p)
        {

        }

    }

}