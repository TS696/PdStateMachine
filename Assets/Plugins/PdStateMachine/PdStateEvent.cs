using System;

namespace PdStateMachine
{
    public abstract class PdStateEvent
    {
        private static readonly PopEvent _popEvent = new PopEvent();

        private static readonly ContinueEvent _continueEvent = new ContinueEvent();

        public static PdStateEvent Pop()
        {
            return _popEvent;
        }

        public static PdStateEvent Continue()
        {
            return _continueEvent;
        }

        public static PdStateEvent PushSubState(PdState state, bool popSelf = false)
        {
            return new PushSubStateEvent(state, popSelf);
        }

        public static PdStateEvent PushSubStates(PdState[] states, bool popSelf = false)
        {
            return new PushSubStatesEvent(states, popSelf);
        }

        public static PdStateEvent PushSubState<T>(bool popSelf = false) where T : PdState
        {
            return new PushRegisteredStateEvent(typeof(T), popSelf);
        }

        public static PdStateEvent RaiseMessage(object message)
        {
            return new RaiseMessageEvent(message);
        }
    }

    internal sealed class ContinueEvent : PdStateEvent
    {
    }

    internal sealed class PushSubStateEvent : PdStateEvent
    {
        public PushSubStateEvent(PdState state, bool popSelf)
        {
            State = state;
            PopSelf = popSelf;
        }

        public PdState State { get; }
        public bool PopSelf { get; }
    }

    internal sealed class PushSubStatesEvent : PdStateEvent
    {
        public PushSubStatesEvent(PdState[] states, bool popSelf)
        {
            States = states;
            PopSelf = popSelf;
        }

        public PdState[] States { get; }
        public bool PopSelf { get; }
    }

    internal sealed class PushRegisteredStateEvent : PdStateEvent
    {
        public PushRegisteredStateEvent(Type type, bool popSelf)
        {
            Type = type;
            PopSelf = popSelf;
        }

        public Type Type { get; }
        public bool PopSelf { get; }
    }

    internal sealed class PopEvent : PdStateEvent
    {
    }

    internal sealed class RaiseMessageEvent : PdStateEvent
    {
        public RaiseMessageEvent(object message)
        {
            Message = message;
        }

        public object Message { get; }
    }
}