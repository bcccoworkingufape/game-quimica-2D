using System.Collections.Generic;
using UnityEngine;
using Domain;
using Core;
using LabScripts;

namespace Presentation.Lab
{
    public class QuestionFlowPresenter : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private LabUIController uiController;
        [SerializeField] private TestManager testManager;

        private SubmitAnswerUseCase _submitAnswerUseCase;
        private GameManager _gm;
        private IHistoryService _historyService;

        private readonly List<Question> _allQuestions = new List<Question>();
        private Question _currentQuestion;
        private int _currentStreak;

        private void Awake()
        {
            if (uiController == null) uiController = FindFirstObjectByType<LabUIController>();
            if (testManager == null) testManager = FindFirstObjectByType<TestManager>();
        }

        private void Start()
        {
            var scoringService = ServiceLocator.Resolve<IScoringService>();
            if (scoringService == null)
            {
                Debug.LogError("[QuestionFlowPresenter] IScoringService não resolvido. Verifique o Bootstrapper.");
                return;
            }

            _historyService = ServiceLocator.Resolve<IHistoryService>();
            if (_historyService == null)
                Debug.LogWarning("[QuestionFlowPresenter] IHistoryService não resolvido (histórico não será limpo).");

            _submitAnswerUseCase = new SubmitAnswerUseCase(scoringService);
            _gm = GameManager.Instance;

            LoadQuestions();

            // informa pro GameManager o total de questões do run
            _gm?.SetTotalQuestions(_allQuestions.Count);

            // restaura a questão ativa se existir (para Restart funcionar)
            PrepareOrRestoreActiveQuestion();
        }

        private void LoadQuestions()
        {
            _allQuestions.Clear();

            _allQuestions.Add(new Question(1, 1, "Q1",
                new List<string> { "Etanoato de etila", "Butanoato de sódio", "Cicloexanona", "Decano-1-amina" },
                "Etanoato de etila", "Dica: é um éster.",
                "Etanoato de etila é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(2, 2, "Q2",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "Cicloexanona", "Decano-1-amina" },
                "Butanoato de sódio", "Dica: Não é o Etanoato de etila.",
                "Butanoato de sódio é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(3, 3, "Q3",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "4-aminobenzenossulfonamida", "Decano-1-amina" },
                "Ácido propanoico", "Dica: é um ácido carboxílico.",
                "Ácido propanoico é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(4, 4, "Q4",
                new List<string> { "Ácido propanoico", "Metilbenzeno", "Cicloexanona", "Butan-1-amina" },
                "Butan-1-amina", "Dica: é uma amina.",
                "Butan-1-amina é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(5, 5, "Q5",
                new List<string> { "Ácido oleico", "4-aminobenzenossulfonamida", "orto-diclorobenzeno", "Decano-1-amina" },
                "Ácido oleico", "Dica: é um ácido graxo.",
                "Ácido oleico é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(6, 6, "Q6",
                new List<string> { "Ácido propanoico", "Butanoato de sódio", "4-aminobenzenossulfonamida", "Decano-1-amina" },
                "4-aminobenzenossulfonamida", "Dica: 4-aminobenzenossulfonamida.",
                "4-aminobenzenossulfonamida é um composto orgânico que forma uma solução incolor quando misturado com água. E afunda na água."));

            _allQuestions.Add(new Question(7, 7, "Q7",
                new List<string> { "Ácido oleico", "4-aminobenzenossulfonamida", "orto-diclorobenzeno", "Decano-1-amina" },
                "Decano-1-amina", "Dica: é uma amina de cadeia longa.",
                "Decano-1-amina é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(8, 8, "Q8",
                new List<string> { "Cicloexanona", "Ácido oleico", "Ácido propanoico", "4-aminobenzenossulfonamida" },
                "Cicloexanona", "Dica: é uma cetona cíclica.",
                "Cicloexanona é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(9, 9, "Q9",
                new List<string> { "Cicloexanona", "Butan-1-amina", "Metilbenzeno", "orto-diclorobenzeno" },
                "Metilbenzeno", "Dica: é um composto aromático.",
                "Metilbenzeno é um composto orgânico que forma uma solução incolor quando misturado com água."));

            _allQuestions.Add(new Question(10, 10, "Q10",
                new List<string> { "Cicloexanona", "Butan-1-amina", "Metilbenzeno", "orto-diclorobenzeno" },
                "orto-diclorobenzeno", "Dica: é um composto aromático com cloro.",
                "orto-diclorobenzeno é um composto orgânico que forma uma solução incolor quando misturado com água."));
        }

        private void PrepareOrRestoreActiveQuestion()
        {
            if (_gm != null && _gm.ActiveQuestionId > 0)
            {
                _currentQuestion = _allQuestions.Find(q => q.Id == _gm.ActiveQuestionId);
                if (_currentQuestion != null)
                {
                    testManager?.SetCurrentCompound(_currentQuestion.CompoundId);
                    return;
                }
            }

            SelectNewQuestion();
        }

        private List<Question> GetAvailableQuestions()
        {
            var list = new List<Question>();
            foreach (var q in _allQuestions)
            {
                if (_gm != null && _gm.IsQuestionCompleted(q.Id))
                    continue;
                list.Add(q);
            }
            return list;
        }

        private void SelectNewQuestion()
        {
            var available = GetAvailableQuestions();
            if (available.Count == 0)
            {
                Debug.Log("[QuestionFlowPresenter] Você respondeu todas as perguntas!");
                _currentQuestion = null;
                return;
            }

            _currentQuestion = available[Random.Range(0, available.Count)];
            _gm?.SetActiveQuestion(_currentQuestion.Id);

            testManager?.SetCurrentCompound(_currentQuestion.CompoundId);
            uiController.questionPanelTitle.text = $"Questão {_currentQuestion.Id}";
            Debug.Log("A questão atual é a de id: " + _currentQuestion.Id);
        }

        public void ShowQuestionForCurrentCompound()
        {
            if (_currentQuestion == null)
                PrepareOrRestoreActiveQuestion();

            if (_currentQuestion == null)
                return;

            uiController?.SetupQuestionPanel(_currentQuestion);
            uiController?.ShowQuestionPanel();
        }

        public void OnAnswerSelected(int optionIndex)
        {
            if (_currentQuestion == null) return;
            if (optionIndex < 0 || optionIndex >= _currentQuestion.Alternatives.Count) return;

            string selectedAnswer = _currentQuestion.Alternatives[optionIndex];

            var request = new SubmitAnswerRequest(_currentQuestion, selectedAnswer, _currentStreak);
            var result = _submitAnswerUseCase.Execute(request);

            _currentStreak = result.NewStreak;

            if (_gm != null)
            {
                if (result.IsCorrect) _gm.AddScore(result.EarnedPoints);
                else _gm.LoseLife();
            }

            if (result.IsCorrect)
            {
                _gm?.MarkActiveQuestionCorrect();
                uiController?.ShowQuestionVictoryPanel(_currentQuestion.CorrectAnswer);
            }
            else
            {
                uiController?.ShowQuestionErrorPanel();
            }
        }

        /// <summary>
        /// Chamado no "Próxima fase": aqui sim remove do run e sorteia a próxima
        /// </summary>
        public void PrepareNextCompound(bool forceNew = true)
        {
            if (_gm != null && _gm.ActiveQuestionAnsweredCorrect)
                _gm.CommitActiveQuestionAsCompleted();

            // se zerou o game, o GameManager já troca de cena
            _historyService?.Clear();
            testManager?.ResetRoundState();

            SelectNewQuestion();
        }
    }
}
