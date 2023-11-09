namespace PdStateMachine
{
    public abstract class PdState
    {
        protected PdStateContext Context { get; private set; }

        internal void SetContext(PdStateContext context)
        {
            Context = context;
        }

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

        public virtual bool HandleMessage(StateMessage message)
        {
#pragma warning disable 0618
            return HandleMessage(message.Message);
#pragma warning restore 0618
        }

        [System.Obsolete("please override `HandleMessage(StateMessage message)` instead.", false)]
        public virtual bool HandleMessage(object message)
        {
            return false;
        }
    }
}