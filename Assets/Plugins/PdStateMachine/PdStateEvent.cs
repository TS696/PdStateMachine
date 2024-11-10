using System;
using UnityEngine.Pool;

namespace PdStateMachine
{
    public readonly struct PdStateEventHandle
    {
        internal readonly long Version;
        internal readonly PdStateEvent Event;

        internal PdStateEventHandle(PdStateEvent evt)
        {
            Version = evt.Version;
            Event = evt;
        }
    }

    public abstract class PdStateEvent
    {
        internal long Version { get; set; }
        
        public static PdStateEventHandle Pop()
        {
            return new PdStateEventHandle(PopEvent.GetInstance());
        }

        public static PdStateEventHandle Continue()
        {
            return new PdStateEventHandle(ContinueEvent.GetInstance());
        }

        public static PdStateEventHandle PushSubState(PdState state, bool popSelf = false)
        {
            return new PdStateEventHandle(PushSubStateEvent.GetInstance(state, popSelf));
        }

        public static PdStateEventHandle PushSubStates(PdState[] states, bool popSelf = false)
        {
            return new PdStateEventHandle(PushSubStatesEvent.GetInstance(states, popSelf));
        }

        public static PdStateEventHandle PushSubState<T>(bool popSelf = false) where T : PdState
        {
            return new PdStateEventHandle(PushRegisteredStateEvent.GetInstance(typeof(T), popSelf));
        }

        public static PdStateEventHandle PushSubStates(Type[] stateTypes, bool popSelf = false)
        {
            return new PdStateEventHandle(PushRegisteredStatesEvent.GetInstance(stateTypes, popSelf));
        }

        public static PdStateEventHandle RaiseMessage<T>(T message) where T : IStateMessage
        {
            return new PdStateEventHandle(RaiseMessageEvent.GetInstance(message));
        }
    }

    internal abstract class PdStateEvent<T> : PdStateEvent where T : PdStateEvent, new()
    {
        private static readonly ObjectPool<T> eventPool = new(createFunc: () => new T(), actionOnRelease: x => x.Version++);

        internal static T GetInstance() => eventPool.Get();
        internal static void ReturnInstance(T instance) => eventPool.Release(instance);
    }

    internal sealed class ContinueEvent : PdStateEvent<ContinueEvent>
    {
    }

    internal sealed class PushSubStateEvent : PdStateEvent<PushSubStateEvent>
    {
        public static PushSubStateEvent GetInstance(PdState state, bool popSelf)
        {
            var instance = GetInstance();
            instance.State = state;
            instance.PopSelf = popSelf;
            return instance;
        }

        public PdState State { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushSubStatesEvent : PdStateEvent<PushSubStatesEvent>
    {
        public static PushSubStatesEvent GetInstance(PdState[] states, bool popSelf)
        {
            var instance = GetInstance();
            instance.States = states;
            instance.PopSelf = popSelf;
            return instance;
        }

        public PdState[] States { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushRegisteredStateEvent : PdStateEvent<PushRegisteredStateEvent>
    {
        public static PushRegisteredStateEvent GetInstance(Type type, bool popSelf)
        {
            var instance = GetInstance();
            instance.Type = type;
            instance.PopSelf = popSelf;
            return instance;
        }

        public Type Type { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PushRegisteredStatesEvent : PdStateEvent<PushRegisteredStatesEvent>
    {
        public static PushRegisteredStatesEvent GetInstance(Type[] stateTypes, bool popSelf)
        {
            var instance = GetInstance();
            instance.StateTypes = stateTypes;
            instance.PopSelf = popSelf;
            return instance;
        }

        public Type[] StateTypes { get; private set; }
        public bool PopSelf { get; private set; }
    }

    internal sealed class PopEvent : PdStateEvent<PopEvent>
    {
    }

    internal sealed class RaiseMessageEvent : PdStateEvent<RaiseMessageEvent>
    {
        public IStateMessageHandler MessageHandler { get; private set; }

        public static RaiseMessageEvent GetInstance<T>(T message) where T : IStateMessage
        {
            var instance = GetInstance();
            var messageHandler = new StateMessageHandler<T>
            {
                Message = message
            };
            instance.MessageHandler = messageHandler;
            return instance;
        }
    }
}
