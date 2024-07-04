namespace PdStateMachine
{
    public interface IStateMessageReceiver<in T> where T : IStateMessage
    {
        bool HandleMessage(T message, PdState sender);
    }
}
