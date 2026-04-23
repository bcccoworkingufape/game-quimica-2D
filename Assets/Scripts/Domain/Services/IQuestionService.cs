using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Serviço que gera perguntas a partir dos compostos.
    /// </summary>
    public interface IQuestionService
    {
        Question CreateMysteryCompoundQuestion(
            Compound target,
            IReadOnlyList<Compound> allCompounds,
            int optionCount = 4);
    }
}
