using System.Collections.Generic;
using UnityEngine;
using Domain;
using Core;
using LabScripts;

namespace Presentation.Lab
{
    /// <summary>
    /// Gerencia o fluxo de perguntas:
    /// - mantém uma lista de questões em memória
    /// - escolhe uma questão aleatória entre as que restam
    /// - avisa o TestManager qual é o compoundId da questão
    /// - valida a resposta e aciona os painéis de vitória/erro
    /// </summary>
    public class QuestionFlowPresenter : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private LabUIController uiController;
        [SerializeField] private TestManager testManager;

        private SubmitAnswerUseCase _submitAnswerUseCase;
        private GameManager _gameManager;

        private readonly List<Question> _remainingQuestions = new List<Question>();
        private Question _currentQuestion;
        private int _currentQuestionIndex = -1;
        private int _currentStreak;

        private void Awake()
        {
            if (uiController == null)
                uiController = FindObjectOfType<LabUIController>();

            if (testManager == null)
                testManager = FindObjectOfType<TestManager>();
        }

        private void Start()
        {
            var scoringService = ServiceLocator.Resolve<IScoringService>();
            if (scoringService == null)
            {
                Debug.LogError("[QuestionFlowPresenter] IScoringService não resolvido. Verifique o Bootstrapper.");
                return;
            }

            _submitAnswerUseCase = new SubmitAnswerUseCase(scoringService);
            _gameManager = GameManager.Instance;

            LoadQuestions();
        }

        // --------------------------------------------------------------------
        // Carrega as questões (hard-coded por enquanto)
        // --------------------------------------------------------------------
        private void LoadQuestions()
        {
            _remainingQuestions.Clear();

            // idQuestao, compoundId, "descricao", alternativas, respostaCorreta, hint, feedback

            _remainingQuestions.Add(new Question(
                1,
                1,
                "Q1",
                new List<string> { "Etanoato de etila", "Butanoato de sódio", "Cicloexanona", "Decano-1-amina" },
                "Etanoato de etila",
                "Dica: é um éster.",
                "Etanoato de etila é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                2,
                2,
                "Q2",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "Cicloexanona", "Decano-1-amina" },
                "Butanoato de sódio",
                "Dica: Não é o Etanoato de etila.",
                "Butanoato de sódio é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                3,
                3,
                "Q3",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "4-aminobenzenossulfonamida", "Decano-1-amina" },
                "Ácido propanoico",
                "Dica: é um ácido carboxílico.",
                "Ácido propanoico é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                4,
                4,
                "Q4",
                new List<string> { "Ácido propanoico", "Metilbenzeno", "Cicloexanona", "Butan-1-amina" },
                "Butan-1-amina",
                "Dica: é uma amina.",
                "Butan-1-amina é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                5,
                5,
                "Q5",
                new List<string> { "Ácido oleico", "4-aminobenzenossulfonamida", "orto-diclorobenzeno", "Decano-1-amina" },
                "Ácido oleico",
                "Dica: é um ácido graxo.",
                "Ácido oleico é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                6,
                6,
                "Q6",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "4-aminobenzenossulfonamida", "Decano-1-amina" },
                "4-aminobenzenossulfonamida",
                "Dica: 4-aminobenzenossulfonamida.",
                "4-aminobenzenossulfonamida é um composto orgânico que forma uma solução incolor quando misturado com água. E afunda na água."
            ));

            _remainingQuestions.Add(new Question(
                7,
                7,
                "Q7",
                new List<string> { "Ácido oleico", "4-aminobenzenossulfonamida", "orto-diclorobenzeno", "Decano-1-amina" },
                "Decano-1-amina",
                "Dica: é uma amina de cadeia longa.",
                "Decano-1-amina é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                8,
                8,
                "Q8",
                new List<string> { "Cicloexanona", "Ácido oleico", "Ácido propanoico", "4-aminobenzenossulfonamida" },
                "Cicloexanona",
                "Dica: é uma cetona cíclica.",
                "Cicloexanona é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                9,
                9,
                "Q9",
                new List<string> { "Cicloexanona", "Butan-1-amina", "Metilbenzeno", "orto-diclorobenzeno" },
                "Metilbenzeno",
                "Dica: é um composto aromático.",
                "Metilbenzeno é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));

            _remainingQuestions.Add(new Question(
                10,
                10,
                "Q10",
                new List<string> { "Cicloexanona", "Butan-1-amina", "Metilbenzeno", "orto-diclorobenzeno" },
                "orto-diclorobenzeno",
                "Dica: é um composto aromático com cloro.",
                "orto-diclorobenzeno é um composto orgânico que forma uma solução incolor quando misturado com água."
            ));
        }

        // --------------------------------------------------------------------
        // Seleção da questão (sempre RANDOM das que restam)
        // --------------------------------------------------------------------
        private void SelectNewQuestion()
        {
            if (_remainingQuestions.Count == 0)
            {
                Debug.Log("Você respondeu a todas as perguntas!! Parabéns");
                _currentQuestion = null;
                _currentQuestionIndex = -1;
                return;
            }

            _currentQuestionIndex = Random.Range(0, _remainingQuestions.Count);
            _currentQuestion = _remainingQuestions[_currentQuestionIndex];

            // LOG: id da questão + id do composto
            Debug.Log($"[QuestionFlowPresenter] Questão selecionada Id={_currentQuestion.Id}, CompoundId={_currentQuestion.CompoundId}");

            // avisa o TestManager qual é o composto misterioso
            if (testManager != null)
            {
                testManager.SetCurrentCompound(_currentQuestion.CompoundId);
            }
        }

        public void ShowQuestionForCurrentCompound()
        {
            if (_remainingQuestions.Count == 0)
            {
                Debug.Log("Você respondeu a todas as perguntas!! Parabéns");
                return;
            }

            if (_currentQuestion == null)
                SelectNewQuestion();

            if (_currentQuestion == null)
                return;

            uiController?.SetupQuestionPanel(_currentQuestion);
            uiController?.ShowQuestionPanel();
        }

        // --------------------------------------------------------------------
        // Resposta do jogador
        // --------------------------------------------------------------------
        public void OnAnswerSelected(int optionIndex)
        {
            if (_currentQuestion == null)
            {
                Debug.LogWarning("[QuestionFlowPresenter] Nenhuma pergunta ativa ao receber resposta.");
                return;
            }

            if (optionIndex < 0 || optionIndex >= _currentQuestion.Alternatives.Count)
            {
                Debug.LogWarning("[QuestionFlowPresenter] Índice de alternativa inválido.");
                return;
            }

            string selectedAnswer = _currentQuestion.Alternatives[optionIndex];

            var request = new SubmitAnswerRequest(_currentQuestion, selectedAnswer, _currentStreak);
            var result = _submitAnswerUseCase.Execute(request);

            _currentStreak = result.NewStreak;

            if (_gameManager != null)
            {
                if (result.IsCorrect)
                    _gameManager.AddScore(result.EarnedPoints);
                else
                    _gameManager.LoseLife();
            }

            Debug.Log($"[QuestionFlowPresenter] Correta? {result.IsCorrect}. Feedback: {result.Feedback}");

            if (result.IsCorrect)
            {
                // guarda o nome ANTES de limpar o estado
                string compoundName = _currentQuestion.CorrectAnswer;

                // remove da lista para não repetir
                if (_currentQuestionIndex >= 0 && _currentQuestionIndex < _remainingQuestions.Count)
                {
                    _remainingQuestions.RemoveAt(_currentQuestionIndex);
                }

                _currentQuestion = null;
                _currentQuestionIndex = -1;

                uiController?.ShowQuestionVictoryPanel(compoundName);
            }
            else
            {
                uiController?.ShowQuestionErrorPanel();
            }
        }

        public void PrepareNextCompound()
        {
            SelectNewQuestion(); // mantém a mesma lógica de sorteio
                                 // NÃO chama uiController.ShowQuestionPanel() aqui
        }
    }
}
