namespace Domain
{
    /// <summary>
    /// Strategy que encapsula regras especificas de cada modo de jogo.
    /// Substitui ifs espalhados (mode == GameMode.X) por polimorfismo.
    /// </summary>
    public interface IGameModeRules
    {
        GameMode Mode { get; }

        /// <summary>Rotulo exibido na UI (ex: "Estudo Livre", "Experimentos", "Desafio").</summary>
        string Label { get; }

        /// <summary>Se a arvore de decisão pode ser consultada.</summary>
        bool AllowsDecisionTree { get; }

        /// <summary>Se errar uma questão consome vida.</summary>
        bool AppliesLifePenalty { get; }

        /// <summary>Texto a exibir em vez de "Vidas: N" (null para usar o padrão).</summary>
        string LivesOverrideText { get; }

        /// <summary>Quantidade de icones de vida/estrela a destacar para uma quantidade dada de vidas.</summary>
        int VisibleLifeIcons(int lives, int maxIcons);
    }
}
