namespace PdStateMachine
{
    public interface IStateMessageHandler
    {
        public bool TryRaise(PdState target, PdState sender);
        IStateMessage RawMessage { get; }
    }

    public class StateMessageHandler<T> : IStateMessageHandler
        where T : IStateMessage
    {
        public T Message { get; set; }
        public IStateMessage RawMessage => Message;

        public bool TryRaise(PdState target, PdState sender)
        {
            if (target is IStateMessageReceiver<T> handler)
            {
                return handler.HandleMessage(Message, sender);
            }

            return false;
        }
    }
}
