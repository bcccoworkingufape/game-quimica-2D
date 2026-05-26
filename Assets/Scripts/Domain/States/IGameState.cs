namespace Domain.States
{
    /// <summary>
    /// Estado lógico de alto nível do jogo (menu, lab, question, mixing, vitoria, derrota).
    /// </summary>
    public interface IGameState
    {
        string Name { get; }
        void OnEnter();
        void OnExit();
    }
}
