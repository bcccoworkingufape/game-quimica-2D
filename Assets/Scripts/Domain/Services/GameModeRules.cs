namespace Domain
{
    /// <summary>Fabrica para resolver o <see cref="IGameModeRules"/> a partir do enum.</summary>
    public static class GameModeRulesFactory
    {
        public static IGameModeRules For(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Desafio:
                    return new DesafioRules();
                case GameMode.Experimentos:
                    return new ExperimentosRules();
                case GameMode.Estudo_Livre:
                    return new EstudoLivreRules();
                default:
                    return new EstudoLivreRules();
            }
        }
    }

    public sealed class EstudoLivreRules : IGameModeRules
    {
        public GameMode Mode => GameMode.Estudo_Livre;
        public string Label => "Estudo Livre";
        public bool AllowsDecisionTree => true;
        public bool AppliesLifePenalty => false;
        public string LivesOverrideText => "Sem penalidade";

        public int VisibleLifeIcons(int lives, int maxIcons) => maxIcons;
    }

    public sealed class ExperimentosRules : IGameModeRules
    {
        public GameMode Mode => GameMode.Experimentos;
        public string Label => "Experimentos";
        public bool AllowsDecisionTree => true;
        public bool AppliesLifePenalty => true;
        public string LivesOverrideText => null;

        public int VisibleLifeIcons(int lives, int maxIcons) => lives;
    }

    public sealed class DesafioRules : IGameModeRules
    {
        public GameMode Mode => GameMode.Desafio;
        public string Label => "Desafio";
        public bool AllowsDecisionTree => false;
        public bool AppliesLifePenalty => true;
        public string LivesOverrideText => null;

        public int VisibleLifeIcons(int lives, int maxIcons) => lives;
    }
}
