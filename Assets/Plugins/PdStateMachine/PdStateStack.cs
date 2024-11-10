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
        public bool TickUntilContinue { get; set; }
        public int LimitTickLoopNum { get; set; } = 1000;

        public event Action<IStateMessage, PdState> UnhandledMessage;

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
            var isContinue = true;
            var loopCount = 0;
            while (isContinue)
            {
                if (_current == null)
                {
                    break;
                }

                switch (_current.Status)
                {
                    case StateStatus.Disable:
                        _current.OnEntry();
                        break;
                    case StateStatus.Pause:
                        _current.OnResume();
                        break;
                }

                var eventHandle = _current.OnTick();
                if (eventHandle.Event == null)
                {
                    throw new InvalidOperationException($"{nameof(PdStateEvent)} should not be null.");
                }

                if (eventHandle.Version != eventHandle.Event.Version)
                {
                    throw new InvalidOperationException("PdStateEvent version mismatch. PdStateEvent cannot be reused.");
                }

                ExecuteEvent(eventHandle.Event);

                isContinue = TickUntilContinue && eventHandle.Event is not ContinueEvent;
                loopCount++;
                if (isContinue && LimitTickLoopNum > 0 && loopCount >= LimitTickLoopNum)
                {
                    throw new InvalidOperationException("Limit tick loop num exceeded.");
                }
            }
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

        public bool RaiseMessage<T>(T message) where T : IStateMessage
        {
            var messageHandler = new StateMessageHandler<T>
            {
                Message = message
            };
            return RaiseMessage(messageHandler, null);
        }

        private bool RaiseMessage(IStateMessageHandler messageHandler, PdState sender)
        {
            while (_processStack.Count > 0)
            {
                var state = _processStack.Peek();
                if (state.HandleMessage(messageHandler, sender))
                {
                    return true;
                }

                PopState();
            }
            
            UnhandledMessage?.Invoke(messageHandler.RawMessage, sender);

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
                case ContinueEvent continueEvent:
                    ContinueEvent.ReturnInstance(continueEvent);
                    break;
                case PushSubStateEvent pushStateEvent:
                    if (pushStateEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushState(pushStateEvent.State);
                    PushSubStateEvent.ReturnInstance(pushStateEvent);
                    break;
                case PushSubStatesEvent pushStatesEvent:
                    if (pushStatesEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushStates(pushStatesEvent.States);
                    PushSubStatesEvent.ReturnInstance(pushStatesEvent);
                    break;
                case PushRegisteredStateEvent pushRegisteredStateEvent:
                    if (pushRegisteredStateEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushState(GetRegisteredState(pushRegisteredStateEvent.Type));
                    PushRegisteredStateEvent.ReturnInstance(pushRegisteredStateEvent);
                    break;
                case PushRegisteredStatesEvent pushRegisteredStatesEvent:
                    if (pushRegisteredStatesEvent.PopSelf)
                    {
                        PopState();
                    }

                    PushStates(pushRegisteredStatesEvent.StateTypes);
                    PushRegisteredStatesEvent.ReturnInstance(pushRegisteredStatesEvent);
                    break;
                case PopEvent popEvent:
                    PopState();
                    PopEvent.ReturnInstance(popEvent);
                    break;

                case RaiseMessageEvent raiseMessageEvent:
                    RaiseMessage(raiseMessageEvent.MessageHandler, _current.State);
                    RaiseMessageEvent.ReturnInstance(raiseMessageEvent);
                    break;
            }
        }

        public override void OnEntry()
        {
        }

        public override PdStateEventHandle OnTick()
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

            public PdStateEventHandle OnTick()
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

            public bool HandleMessage(IStateMessageHandler handler, PdState sender)
            {
                _state.SetContext(_context);
                return handler.TryRaise(_state, sender);
            }
        }
    }
}
