namespace DigitalLove.Game.Planets
{
    public class IdCreator
    {
        private int idCount = 1;
        public string NextId { get { idCount++; return idCount.ToString("0000"); } }
    }
}