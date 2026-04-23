using System;

namespace Domain
{
    public readonly struct GenerateMysteryQuestionRequest
    {
        public int CompoundId { get; }

        public GenerateMysteryQuestionRequest(int compoundId)
        {
            CompoundId = compoundId;
        }
    }

    public readonly struct GenerateMysteryQuestionResponse
    {
        public Question Question { get; }

        public GenerateMysteryQuestionResponse(Question question)
        {
            Question = question;
        }
    }

    /// <summary>
    /// Gera a pergunta de múltipla escolha para o composto misterioso atual.
    /// </summary>
    public class GenerateMysteryQuestionUseCase
    {
        private readonly ICompoundRepository _compoundRepository;
        private readonly IQuestionService _questionService;

        public GenerateMysteryQuestionUseCase(
            ICompoundRepository compoundRepository,
            IQuestionService questionService)
        {
            _compoundRepository = compoundRepository
                ?? throw new ArgumentNullException(nameof(compoundRepository));
            _questionService = questionService
                ?? throw new ArgumentNullException(nameof(questionService));
        }

        public GenerateMysteryQuestionResponse Execute(GenerateMysteryQuestionRequest request)
        {
            var target = _compoundRepository.GetById(request.CompoundId);
            if (target == null)
            {
                throw new InvalidOperationException(
                    $"Composto com id {request.CompoundId} não encontrado.");
            }

            var all = _compoundRepository.ListAll();
            var question = _questionService.CreateMysteryCompoundQuestion(target, all);

            return new GenerateMysteryQuestionResponse(question);
        }
    }
}
