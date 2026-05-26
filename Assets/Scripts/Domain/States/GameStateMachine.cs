using System;
using System.Collections.Generic;

namespace Domain.States
{
    /// <summary>
    /// FSM leve para o jogo. Não cria estados; estados são registrados pelo bootstrap.
    /// Transições disparam eventos para que a camada de apresentação reaja
    /// (carregar cena, abrir painel, etc.) sem acoplar a FSM ao Unity.
    /// </summary>
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IGameState> _states = new Dictionary<Type, IGameState>();

        public IGameState CurrentState { get; private set; }

        public event Action<IGameState, IGameState> OnStateChanged;

        public void Register<T>(T state) where T : IGameState
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[typeof(T)] = state;
        }

        public bool TryGet<T>(out T state) where T : IGameState
        {
            if (_states.TryGetValue(typeof(T), out var s))
            {
                state = (T)s;
                return true;
            }
            state = default;
            return false;
        }

        public void Transition<T>() where T : IGameState
        {
            if (!_states.TryGetValue(typeof(T), out var next))
                throw new InvalidOperationException($"State {typeof(T).Name} not registered.");

            if (ReferenceEquals(next, CurrentState))
                return;

            var previous = CurrentState;
            previous?.OnExit();
            CurrentState = next;
            CurrentState.OnEnter();
            OnStateChanged?.Invoke(previous, CurrentState);
        }
    }
}
