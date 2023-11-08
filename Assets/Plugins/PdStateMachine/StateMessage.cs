namespace PdStateMachine
{
    public readonly struct StateMessage
    {
        public readonly PdState Sender;
        public readonly object Message;

        public StateMessage(PdState sender, object message)
        {
            Sender = sender;
            Message = message;
        }
    }
}