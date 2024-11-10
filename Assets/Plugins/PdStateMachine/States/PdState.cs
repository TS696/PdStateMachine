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
        
        public virtual PdStateEventHandle OnTick()
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
    }
}