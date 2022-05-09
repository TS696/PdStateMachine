using System;
using System.Collections.Generic;
using System.Linq;

namespace PdStateMachine
{
    public class PdStateStack : PdState, IDisposable
    {
        private readonly Stack<PdStateHolder> _processStack = new Stack<PdStateHolder>();
        private readonly Stack<PdStateHolder> _holderPool = new Stack<PdStateHolder>(5);
        private readonly Dictionary<Type, PdState> _stateInstances = new Dictionary<Type, PdState>();

        private PdStateHolder _current;

        public int ProcessCount => _processStack.Count;

        public void RegisterState<T>(PdState state) where T : PdState
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
            foreach (var state in states.Reverse())
            {
                PushState(state);
            }
        }

        public void PushState<T>() where T : PdState
        {
            PushState(GetRegisteredState(typeof(T)));
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
            ExecuteEvent(evt);
        }

        private void PopState()
        {
            if (_processStack.Count <= 0)
            {
                return;
            }

            var top = _processStack.Pop();
            top.OnExit();
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

        public bool RaiseMessage(object param)
        {
            while (_processStack.Count > 0)
            {
                var state = _processStack.Peek();
                if (state.Status == StateStatus.Disable)
                {
                    state.OnEntry();
                }

                if (state.Status == StateStatus.Pause)
                {
                    state.OnResume();
                }

                if (state.HandleMessage(param))
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
                case PopEvent _:
                    PopState();
                    break;

                case RaiseMessageEvent raiseMessageEvent:
                    RaiseMessage(raiseMessageEvent.Message);
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

        public override bool HandleMessage(object message)
        {
            return RaiseMessage(message);
        }

        private enum StateStatus
        {
            Disable,
            Active,
            Pause
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
            private PdState _state;

            public void Initialize(PdState state)
            {
                _state = state;
                Status = StateStatus.Disable;
            }

            public StateStatus Status { get; private set; }

            public void OnEntry()
            {
                _state.OnEntry();
                Status = StateStatus.Active;
            }

            public PdStateEvent OnTick()
            {
                var evt = _state.OnTick();
                return evt;
            }

            public void OnExit()
            {
                _state.OnExit();
                Status = StateStatus.Disable;
            }

            public void OnPause()
            {
                _state.OnPause();
                Status = StateStatus.Pause;
            }

            public void OnResume()
            {
                _state.OnResume();
                Status = StateStatus.Active;
            }

            public bool HandleMessage(object message)
            {
                return _state.HandleMessage(message);
            }
        }
    }
}