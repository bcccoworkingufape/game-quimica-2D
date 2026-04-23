namespace Domain
{
    public class PlayerProfile
    {
        public string PlayerName { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }

        public PlayerProfile(string playerName, int lives)
        {
            PlayerName = playerName;
            Lives = lives;
            Score = 0;
        }

        public void AddScore(int amount) => Score += amount;

        public void LoseLife()
        {
            if (Lives > 0) Lives--;
        }

        public void Reset(string playerName, int lives)
        {
            PlayerName = playerName;
            Lives = lives;
            Score = 0;
        }
    }
}
