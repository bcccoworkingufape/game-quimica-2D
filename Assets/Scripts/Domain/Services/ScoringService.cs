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

        public int ComputePoints(bool isCorrect, int streak, float difficultyMultiplier = 1f)
        {
            if (!isCorrect) return 0;

            int basePoints = _basePoints + (streak * 10);

            // Mesma ordem de operacoes do Estado C: o base ja e inteiro e o
            // truncamento acontece DEPOIS da multiplicacao, nao arredondando.
            return (int)(basePoints * difficultyMultiplier);
        }
    }
}
