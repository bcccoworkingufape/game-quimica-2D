using UnityEngine;
using Domain;

namespace Data
{
    public class DifficultyLevelData : ScriptableObject
    {
        [Tooltip("Nome da dificuldade, ex: Fácil")]
        public string difficultyName;

        [Tooltip("Número de vidas iniciais para esta dificuldade")]
        public int startingLives = 3;

        [Tooltip("Dicas disponíveis no início do nível")]
        public int availableHints = 1;

        [Tooltip("Multiplicador de pontos para esta dificuldade")]
        public float scoreMultiplier = 1.0f;

        [Header("Modo")]
        [Tooltip("Modo de jogo associado a esta dificuldade")]
        public GameMode mode = GameMode.Estudo_Livre;

        public string ModeLabel => mode switch
        {
            GameMode.Estudo_Livre => "Estudos",
            GameMode.Experimentos => "Laboratório",
            GameMode.Desafio => "Desafio",
            _ => mode.ToString()
        };
    }
}
