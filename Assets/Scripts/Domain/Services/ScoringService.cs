namespace Domain
{
    /// <summary>
    /// Regra de pontuação simples (pode ser refinada depois).
    /// </summary>
    public class ScoringService : IScoringService
    {
        private readonly int _basePoints;

        public ScoringService(int basePoints = 100)
        {
            _basePoints = basePoints;
        }

        public int ComputePoints(bool isCorrect, int streak)
        {
            if (!isCorrect) return 0;
            return _basePoints + (streak * 10);
        }
    }
}
