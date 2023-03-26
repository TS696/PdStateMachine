using System;

namespace PdStateMachine
{
    public abstract class PdStateEvent
    {
        public static PdStateEvent Pop()
        {
            return PopEvent.GetInstance();
        }

        public static PdStateEvent Continue()
        {
            return ContinueEvent.GetInstance();
        }

        public static PdStateEvent PushSubState(PdState state, bool popSelf = false)
        {
            return PushSubStateEvent.GetInstance(state, popSelf);
        }

        public static PdStateEvent PushSubStates(PdState[] states, bool popSelf = false)
        {
            return PushSubStatesEvent.GetInstance(states, popSelf);
        }

        public static PdStateEvent PushSubState<T>(bool popSelf = false) where T : PdState
        {
            return PushRegisteredStateEvent.GetInstance(typeof(T), popSelf);
        }

        public static PdStateEvent PushSubStates(Type[] stateTypes, bool popSelf = false)
        {
            return PushRegisteredStatesEvent.GetInstance(stateTypes, popSelf);
        }

        public static PdStateEvent RaiseMessage(object message)
        {
            return RaiseMessageEvent.GetInstance(message);
        }
    }

    internal sealed class ContinueEvent : PdStateEvent
    {
        private static readonly ContinueEvent _instance = new ContinueEvent();

        public static ContinueEvent GetInstance()
        {
            return _instance;
        }
    }

    internal sealed class PushSubStateEvent : PdStateEvent
    {
        private static readonly PushSubStateEvent _instance = new PushSubStateEvent();

        public static PushSubStateEvent GetInstance(PdState state, bool popSelf)
        {
            _instance.State = state;
            _instance.PopSelf = popSelf;
            return _instance;
        }

        public PdState State { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushSubStatesEvent : PdStateEvent
    {
        private static readonly PushSubStatesEvent _instance = new PushSubStatesEvent();

        public static PushSubStatesEvent GetInstance(PdState[] states, bool popSelf)
        {
            _instance.States = states;
            _instance.PopSelf = popSelf;
            return _instance;
        }

        public PdState[] States { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushRegisteredStateEvent : PdStateEvent
    {
        private static readonly PushRegisteredStateEvent _instance = new PushRegisteredStateEvent();

        public static PushRegisteredStateEvent GetInstance(Type type, bool popSelf)
        {
            _instance.Type = type;
            _instance.PopSelf = popSelf;
            return _instance;
        }

        public Type Type { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushRegisteredStatesEvent : PdStateEvent
    {
        private static readonly PushRegisteredStatesEvent _instance = new PushRegisteredStatesEvent();

        public static PushRegisteredStatesEvent GetInstance(Type[] stateTypes, bool popSelf)
        {
            _instance.StateTypes = stateTypes;
            _instance.PopSelf = popSelf;
            return _instance;
        }

        public Type[] StateTypes { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PopEvent : PdStateEvent
    {
        private static readonly PopEvent _instance = new PopEvent();

        public static PopEvent GetInstance()
        {
            return _instance;
        }
    }

    internal sealed class RaiseMessageEvent : PdStateEvent
    {
        private static readonly RaiseMessageEvent _instance = new RaiseMessageEvent();

        public static RaiseMessageEvent GetInstance(object message)
        {
            _instance.Message = message;
            return _instance;
        }

        public object Message { get; private set; }
    }
}