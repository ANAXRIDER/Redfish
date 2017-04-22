namespace HREngine.Bots
{

    public abstract class Behavior
    {
        public virtual float getPlayfieldValue(Playfield p)
        {
            return 0;
        }

        public virtual float getPlayfieldValueEnemy(Playfield p)
        {
            return getPlayfieldValue(p);
        }

        public virtual float getOwnMinionValue(Minion m, Playfield p)
        {
            return 0;
        }

        public virtual float getEnemyMinionValue(Minion m, Playfield p)
        {
            return 0;
        }

    }

}