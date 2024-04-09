using System;
using System.Collections.Generic;

namespace PdStateMachine
{
    public class PdStateStack : PdState, IDisposable
    {
        private readonly Stack<PdStateHolder> _processStack;
        private readonly Stack<PdStateHolder> _holderPool;
        private readonly Dictionary<Type, PdState> _stateInstances;

        private PdStateHolder _current;

        public int ProcessCount => _processStack.Count;

        public PdStateStack() : this(5)
        {
        }

        public PdStateStack(int stackCapacity)
        {
            _processStack = new Stack<PdStateHolder>(stackCapacity);
            _holderPool = new Stack<PdStateHolder>(stackCapacity);
            _stateInstances = new Dictionary<Type, PdState>();

            for (var i = 0; i < stackCapacity; i++)
            {
                _holderPool.Push(new PdStateHolder());
            }
        }

        public void RegisterState<T>(T state) where T : PdState
        {
            _stateInstances.Add(typeof(T), state);
        }

        private PdState GetRegisteredState(Type type)
        {
            return _stateInstances[type];
        }

        public void PushState(PdState state)
        {
            if (_current != null && _current.Status == StateStatus.Active)
            {
                _current.OnPause();
            }

            var stateHolder = GetHolder();
            stateHolder.Initialize(state);
            _current = stateHolder;
            _processStack.Push(stateHolder);
        }

        public void PushStates(params PdState[] states)
        {
            for (var i = states.Length - 1; i >= 0; i--)
            {
                PushState(states[i]);
            }
        }

        public void PushState<T>() where T : PdState
        {
            PushState(GetRegisteredState(typeof(T)));
        }

        public void PushStates(params Type[] stateTypes)
        {
            for (var i = stateTypes.Length - 1; i >= 0; i--)
            {
                PushState(GetRegisteredState(stateTypes[i]));
            }
        }

        public void Tick()
        {
            if (_current == null)
            {
                return;
            }

            switch (_current.Status)
            {
                case StateStatus.Disable:
                    _current?.OnEntry();
                    break;
                case StateStatus.Pause:
                    _current.OnResume();
                    break;
            }

            var evt = _current?.OnTick();
            if (evt == null)
            {
                throw new InvalidOperationException($"{nameof(PdStateEvent)} should not be null.");
            }
            ExecuteEvent(evt);
        }

        private void PopState()
        {
            if (_processStack.Count <= 0)
            {
                return;
            }

            var top = _processStack.Pop();
            if (top.Status is StateStatus.Active or StateStatus.Pause)
            {
                top.OnExit();
            }

            ReturnHolder(top);

            if (_processStack.Count > 0)
            {
                _current = _processStack.Peek();
            }
            else
            {
                _current = null;
            }
        }

        public void PopAllStates()
        {
            while (_processStack.Count > 0)
            {
                PopState();
            }
        }

        public bool RaiseMessage(object message)
        {
            var stateMessage = new StateMessage(null, message);
            return RaiseMessage(stateMessage);
        }

        private bool RaiseMessage(StateMessage stateMessage)
        {
            while (_processStack.Count > 0)
            {
                var state = _processStack.Peek();
                if (state.HandleMessage(stateMessage))
                {
                    return true;
                }

                PopState();
            }

            return false;
        }

        public void Dispose()
        {
            PopAllStates();
        }

        private void ExecuteEvent(PdStateEvent pdStateEvent)
        {
            switch (pdStateEvent)
            {
                case ContinueEvent _:
                    break;
                case PushSubStateEvent pushStateEvent:
                    if (pushStateEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushState(pushStateEvent.State);
                    break;
                case PushSubStatesEvent pushStatesEvent:
                    if (pushStatesEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushStates(pushStatesEvent.States);
                    break;
                case PushRegisteredStateEvent pushRegisteredStateEvent:
                    if (pushRegisteredStateEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushState(GetRegisteredState(pushRegisteredStateEvent.Type));
                    break;
                case PushRegisteredStatesEvent pushRegisteredStatesEvent:
                    if (pushRegisteredStatesEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushStates(pushRegisteredStatesEvent.StateTypes);
                    break;
                case PopEvent _:
                    PopState();
                    break;

                case RaiseMessageEvent raiseMessageEvent:
                    RaiseMessage(new StateMessage(_current.State, raiseMessageEvent.Message));
                    break;
            }
        }

        public override void OnEntry()
        {
        }

        public override PdStateEvent OnTick()
        {
            if (ProcessCount <= 0)
            {
                return PdStateEvent.Pop();
            }

            Tick();
            return PdStateEvent.Continue();
        }

        public override void OnExit()
        {
            Dispose();
        }

        public override void OnPause()
        {
            _current?.OnPause();
        }

        public override void OnResume()
        {
            _current?.OnResume();
        }

        public override bool HandleMessage(StateMessage message)
        {
            return RaiseMessage(message);
        }

        private PdStateHolder GetHolder()
        {
            if (_holderPool.Count > 0)
            {
                return _holderPool.Pop();
            }

            return new PdStateHolder();
        }

        private void ReturnHolder(PdStateHolder holder)
        {
            _holderPool.Push(holder);
        }

        private class PdStateHolder
        {
            public PdState State => _state;
            private PdState _state;

            private readonly PdStateContext _context = new();
            public StateStatus Status => _context.Status;

            public void Initialize(PdState state)
            {
                _state = state;
                _context.Status = StateStatus.Disable;
            }

            public void OnEntry()
            {
                _state.SetContext(_context);
                _state.OnEntry();
                _context.Status = StateStatus.Active;
            }

            public PdStateEvent OnTick()
            {
                var evt = _state.OnTick();
                return evt;
            }

            public void OnExit()
            {
                _state.OnExit();
                _context.Status = StateStatus.Disable;
            }

            public void OnPause()
            {
                _state.OnPause();
                _context.Status = StateStatus.Pause;
            }

            public void OnResume()
            {
                _state.SetContext(_context);
                _state.OnResume();
                _context.Status = StateStatus.Active;
            }

            public bool HandleMessage(StateMessage message)
            {
                _state.SetContext(_context);
                return _state.HandleMessage(message);
            }
        }
    }
}