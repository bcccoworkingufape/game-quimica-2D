using System;
using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Implementação padrão de geração de perguntas para o "composto misterioso".
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly Random _random = new Random();

        public Question CreateMysteryCompoundQuestion(
            Compound target,
            IReadOnlyList<Compound> allCompounds,
            int optionCount = 4)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (allCompounds == null) throw new ArgumentNullException(nameof(allCompounds));

            if (optionCount < 2)
                optionCount = 2;

            // Não pedir mais opções do que compostos disponíveis
            optionCount = Math.Min(optionCount, allCompounds.Count);

            var alternatives = new List<string>
            {
                target.Name // sempre inclui a correta
            };

            // candidatos "distratores"
            var candidates = new List<Compound>();
            foreach (var c in allCompounds)
            {
                if (c.Id != target.Id)
                    candidates.Add(c);
            }

            Shuffle(candidates);

            for (int i = 0; i < optionCount - 1 && i < candidates.Count; i++)
            {
                alternatives.Add(candidates[i].Name);
            }

            // embaralha a lista final de alternativas
            Shuffle(alternatives);

            // Descrição não é usada na UI, mas deixamos preenchida para o domínio.
            string description =
                "Com base nos testes de solubilidade, escolha o composto orgânico misterioso.";
            string hint = $"Dica: ele pertence ao grupo {target.Group}.";
            string feedback = $"O composto correto é {target.Name}, do grupo {target.Group}.";

            return new Question(
                id: target.Id,
                compoundId: target.Id,
                description: description,
                correctAnswer: target.Name,
                alternatives: alternatives,
                hint: hint,
                feedback: feedback);
        }

        private void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = _random.Next(i, n);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
