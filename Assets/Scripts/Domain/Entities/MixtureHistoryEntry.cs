using System;

namespace Domain
{
    /// <summary>
    /// Registro de uma mistura realizada no laboratório.
    /// </summary>
    public class MixtureHistoryEntry
    {
        public int Order { get; }
        public DateTime Timestamp { get; }
        public SolubilityOutcome Outcome { get; }

        public MixtureHistoryEntry(int order, DateTime timestamp, SolubilityOutcome outcome)
        {
            Order = order;
            Timestamp = timestamp;
            Outcome = outcome;
        }

        public override string ToString()
        {
            return $"#{Order} [{Timestamp:HH:mm:ss}] " +
                   $"{Outcome.Compound.Name} + {Outcome.Solvent.Name} → " +
                   $"{Outcome.SolubilityResult} ({Outcome.MixtureType}) / " +
                   $"Litmus: {Outcome.LitmusResult}";
        }
    }
}
