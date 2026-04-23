using System.Collections.Generic;

namespace Domain
{
    public interface IHistoryService
    {
        /// <summary>Registra uma mistura realizada e devolve a entrada criada.</summary>
        MixtureHistoryEntry Register(SolubilityOutcome outcome);

        /// <summary>Retorna todo o histórico em memória.</summary>
        IReadOnlyList<MixtureHistoryEntry> GetAll();

        /// <summary>Limpa o histórico (ex: ao iniciar um novo jogo).</summary>
        void Clear();
    }
}
