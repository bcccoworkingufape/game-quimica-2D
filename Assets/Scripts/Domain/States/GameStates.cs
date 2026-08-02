using System;

namespace Domain.States
{
    /// <summary>
    /// Estado leve, definido por delegates. Permite registrar estados sem
    /// criar uma classe por estado.
    /// </summary>
    public class DelegateGameState : IGameState
    {
        private readonly Action _onEnter;
        private readonly Action _onExit;

        public string Name { get; }

        public DelegateGameState(string name, Action onEnter = null, Action onExit = null)
        {
            Name = name ?? "Unnamed";
            _onEnter = onEnter;
            _onExit = onExit;
        }

        public void OnEnter() => _onEnter?.Invoke();
        public void OnExit() => _onExit?.Invoke();
    }

    // Marker types para Transition<T>().
    public sealed class MenuState : DelegateGameState
    {
        public MenuState(Action onEnter = null, Action onExit = null) : base("Menu", onEnter, onExit) { }
    }

    public sealed class LabState : DelegateGameState
    {
        public LabState(Action onEnter = null, Action onExit = null) : base("Lab", onEnter, onExit) { }
    }

    public sealed class QuestionState : DelegateGameState
    {
        public QuestionState(Action onEnter = null, Action onExit = null) : base("Question", onEnter, onExit) { }
    }

    public sealed class MixingState : DelegateGameState
    {
        public MixingState(Action onEnter = null, Action onExit = null) : base("Mixing", onEnter, onExit) { }
    }

    public sealed class VictoryState : DelegateGameState
    {
        public VictoryState(Action onEnter = null, Action onExit = null) : base("Victory", onEnter, onExit) { }
    }

    public sealed class DefeatState : DelegateGameState
    {
        public DefeatState(Action onEnter = null, Action onExit = null) : base("Defeat", onEnter, onExit) { }
    }

    public sealed class WinGameState : DelegateGameState
    {
        public WinGameState(Action onEnter = null, Action onExit = null) : base("WinGame", onEnter, onExit) { }
    }
}
