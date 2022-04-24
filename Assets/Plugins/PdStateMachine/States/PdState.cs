namespace PdStateMachine
{
    public abstract class PdState
    {
        public virtual void OnEntry()
        {
        }

        public virtual PdStateEvent OnTick()
        {
            return PdStateEvent.Continue();
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual bool HandleMessage(object message)
        {
            return false;
        }
    }
}