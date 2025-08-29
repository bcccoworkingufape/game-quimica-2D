using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "NewDifficultyLevel", menuName = "ChemistryLab/Difficulty Level")]
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
    }
}