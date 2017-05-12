using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_UNG_999t3 : SimTemplate //* Flaming Claws
	{
        //+3 Attack

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            CardDB.Card lpc = CardDB.Instance.getCardDataFromID(p.LastPlayedCard);
            Minion adaptedtarget = new Minion();
            switch (p.LastPlayedCard)
            {
                //self
                case CardDB.cardIDEnum.UNG_047://Ravenous Pterrordax
                case CardDB.cardIDEnum.UNG_109://Elder Longneck
                case CardDB.cardIDEnum.UNG_002://Volcanosaur
                case CardDB.cardIDEnum.UNG_001://Pterrordax Hatchling
                case CardDB.cardIDEnum.UNG_925://Ornery Direhorn
                case CardDB.cardIDEnum.UNG_100://Verdant Longneck
                case CardDB.cardIDEnum.UNG_009://Ravasaur Runt
                case CardDB.cardIDEnum.UNG_082://Thunder Lizard
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            if (m.name == lpc.name && m.playedThisTurn)
                            {
                                adaptedtarget = m;
                                break;
                            }
                        }
                        if (adaptedtarget != null) p.minionGetBuffed(adaptedtarget, 3, 0);
                        break;
                    }
                //silverhand
                case CardDB.cardIDEnum.UNG_962://Lightfused Stegodon
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            if (m.name == CardDB.cardName.silverhandrecruit)
                            {
                                p.minionGetBuffed(m, 3, 0);
                            }
                        }
                        break;
                    }
                //murlocs
                case CardDB.cardIDEnum.UNG_089://Gentle Megasaur
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            if (m.handcard.card.race == TAG_RACE.MURLOC)
                            {
                                p.minionGetBuffed(m, 3, 0);
                            }
                        }
                        break;
                    }
                //every own minion
                case CardDB.cardIDEnum.UNG_103://Evolving Spores
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            p.minionGetBuffed(m, 3, 0);
                        }
                        break;
                    }
                //target
                case CardDB.cardIDEnum.UNG_961://Adaptation
                //own beast target
                case CardDB.cardIDEnum.UNG_915://Crackling Razormaw
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            if (m.entityID == p.adaptTargetEntity)
                            {
                                p.minionGetBuffed(m, 3, 0);
                            }
                        }
                        break;
                    }
                // heroattack, self
                case CardDB.cardIDEnum.UNG_075://Vicious Fledgling
                    {
                        foreach (Minion m in p.ownMinions)
                        {
                            if (p.adaptTargetEntity >= 1 && p.adaptTargetEntity == m.entityID)
                            {
                                adaptedtarget = m;
                                break;
                            }
                        }
                        if (adaptedtarget != null) p.minionGetBuffed(adaptedtarget, 3, 0);
                        break;
                    }
                default: break;
            }
        }
    }
}