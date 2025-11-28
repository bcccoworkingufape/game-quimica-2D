using System;
using System.Collections.Generic;

namespace Domain
{
    /// <summary>
    /// Implementação simples em memória para o histórico de misturas.
    /// </summary>
    public class InMemoryHistoryService : IHistoryService
    {
        private readonly List<MixtureHistoryEntry> _entries = new List<MixtureHistoryEntry>();
        private int _nextOrder = 1;

        public MixtureHistoryEntry Register(SolubilityOutcome outcome)
        {
            if (outcome == null)
                throw new ArgumentNullException(nameof(outcome));

            var entry = new MixtureHistoryEntry(
                order: _nextOrder++,
                timestamp: DateTime.Now,
                outcome: outcome
            );

            _entries.Add(entry);
            return entry;
        }

        public IReadOnlyList<MixtureHistoryEntry> GetAll() => _entries;

        public void Clear()
        {
            _entries.Clear();
            _nextOrder = 1;
        }
    }
}
