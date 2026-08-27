using System.Collections.Generic;
using UnityEngine;
using Domain;
using Data;
using Core;
using LabScripts;
using Core.Audio;


namespace Presentation.Lab
{
    public class QuestionFlowPresenter : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private LabUIController uiController;
        [SerializeField] private MixingRoundController mixingRoundController;

        private SubmitAnswerUseCase _submitAnswerUseCase;
        private GameManager _gm;
        private IHistoryService _historyService;
        private IAudioService _audio;

        private readonly List<Question> _allQuestions = new List<Question>();
        private Question _currentQuestion;
        private int _currentStreak;

        private void Awake()
        {
            if (uiController == null) uiController = FindAnyObjectByType<LabUIController>();
            if (mixingRoundController == null) mixingRoundController = FindAnyObjectByType<MixingRoundController>();
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
            // Fallback identico ao de LabUIController.Awake: audio ausente
            // continua sendo no-op silencioso, nunca excecao.
            _audio = ServiceLocator.TryResolve<IAudioService>(out var a)
                ? a : new UnityAudioService();
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

            var repo = ServiceLocator.Resolve<IQuestionRepository>();
            if (repo == null)
            {
                Debug.LogError("[QuestionFlowPresenter] IQuestionRepository não resolvido. Verifique o Bootstrapper.");
                return;
            }

            foreach (var q in repo.ListAll())
                _allQuestions.Add(q);
        }

        private void PrepareOrRestoreActiveQuestion()
        {
            if (_gm != null && _gm.ActiveQuestionId > 0)
            {
                _currentQuestion = _allQuestions.Find(q => q.Id == _gm.ActiveQuestionId);
                if (_currentQuestion != null)
                {
                    mixingRoundController?.SetCurrentCompound(_currentQuestion.CompoundId);
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

            mixingRoundController?.SetCurrentCompound(_currentQuestion.CompoundId);
            Debug.Log("A questão atual é a de id: " + _currentQuestion.Id);
        }

        public void ShowQuestionForCurrentCompound()
        {
            if (_currentQuestion == null)
                PrepareOrRestoreActiveQuestion();

            if (_currentQuestion == null)
                return;

            uiController.questionPanelTitle.text = $"Questão {_currentQuestion.Id}";
            uiController?.SetupQuestionPanel(_currentQuestion);
            uiController?.ShowQuestionPanel();
        }

        public void OnAnswerSelected(int optionIndex)
        {
            if (_currentQuestion == null) return;
            if (optionIndex < 0 || optionIndex >= _currentQuestion.Alternatives.Count) return;

            string selectedAnswer = _currentQuestion.Alternatives[optionIndex];

            // O multiplicador sai do ScriptableObject de dificuldade e viaja no
            // request. Mesmo fallback de GameManager.cs:228: 1f quando nao ha
            // dificuldade definida.
            float multiplier = _gm != null && _gm.CurrentDifficulty != null
                ? _gm.CurrentDifficulty.scoreMultiplier
                : 1f;

            var request = new SubmitAnswerRequest(
                _currentQuestion, selectedAnswer, _currentStreak, multiplier);
            var result = _submitAnswerUseCase.Execute(request);

            _currentStreak = result.NewStreak;

            if (_gm != null)
            {
                if (result.IsCorrect) _gm.AddScore(result.EarnedPoints);
                else _gm.LoseLife();
            }

            if (result.IsCorrect)
            {
                _audio?.PlayCorrect();
                _gm?.MarkActiveQuestionCorrect();
                uiController?.ShowQuestionVictoryPanel(_currentQuestion.CorrectAnswer);
            }
            else
            {
                _audio?.PlayWrong();
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
            mixingRoundController?.ResetRoundState();

            SelectNewQuestion();
        }
    }
}