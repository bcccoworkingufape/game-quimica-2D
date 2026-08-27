namespace Domain
{
    public interface IScoringService
    {
        /// <summary>
        /// Pontuacao final da resposta, ja com o multiplicador de dificuldade
        /// aplicado. Antes da Etapa E1 o multiplicador era aplicado fora do
        /// dominio, no GameManager, deixando a regra partida em duas camadas.
        /// </summary>
        int ComputePoints(bool isCorrect, int streak, float difficultyMultiplier = 1f);
    }
}
