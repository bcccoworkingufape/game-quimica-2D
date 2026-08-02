using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// Tracker do "run" de perguntas. Mantem quais questões ja foram concluidas,
    /// qual esta ativa, se foi respondida corretamente, e expede o progresso.
    /// Extraido do <see cref="GameManager"/> para isolar a responsabilidade.
    /// </summary>
    public class QuestionRunTracker
    {
        private readonly HashSet<int> _completedQuestionIds = new HashSet<int>();

        public int TotalQuestionsInRun { get; private set; }
        public int ActiveQuestionId { get; private set; }
        public bool ActiveQuestionAnsweredCorrect { get; private set; }

        public event Action<int> OnProgressChanged;
        public event Action OnRunCleared;

        public int CompletedCount => _completedQuestionIds.Count;

        public void SetTotal(int total)
        {
            TotalQuestionsInRun = System.Math.Max(0, total);
            EmitProgress();
        }

        public int GetProgressPercentage()
        {
            if (TotalQuestionsInRun <= 0) return 0;
            return (int)System.Math.Round((double)_completedQuestionIds.Count / TotalQuestionsInRun * 100);
        }

        public bool IsCompleted(int questionId) => _completedQuestionIds.Contains(questionId);

        public void SetActive(int questionId) => ActiveQuestionId = questionId;

        public void MarkActiveCorrect() => ActiveQuestionAnsweredCorrect = true;

        /// <summary>
        /// Commita a questão ativa como concluida (chamado em "Proxima fase").
        /// Retorna true se com isso o run foi totalmente concluido.
        /// </summary>
        public bool Commit()
        {
            if (ActiveQuestionId > 0)
                _completedQuestionIds.Add(ActiveQuestionId);

            ActiveQuestionId = 0;
            ActiveQuestionAnsweredCorrect = false;

            EmitProgress();

            bool cleared = TotalQuestionsInRun > 0 && _completedQuestionIds.Count >= TotalQuestionsInRun;
            if (cleared) OnRunCleared?.Invoke();
            return cleared;
        }

        public void Reset()
        {
            _completedQuestionIds.Clear();
            TotalQuestionsInRun = 0;
            ActiveQuestionId = 0;
            ActiveQuestionAnsweredCorrect = false;
            OnProgressChanged?.Invoke(0);
        }

        private void EmitProgress() => OnProgressChanged?.Invoke(GetProgressPercentage());
    }
}
