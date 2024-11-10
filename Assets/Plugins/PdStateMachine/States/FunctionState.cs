using System;

namespace PdStateMachine
{
    public class FunctionState : PdState
    {
        private readonly Action _onEntry;
        private readonly Func<PdStateEventHandle> _onTick;
        private readonly Action _onExit;
        private readonly Action _onPause;
        private readonly Action _onResume;

        public FunctionState(Action onEntry = null, Func<PdStateEventHandle> onTick = null, Action onExit = null,
            Action onPause = null, Action onResume = null)
        {
            _onEntry = onEntry;
            _onTick = onTick;
            _onExit = onExit;
            _onPause = onPause;
            _onResume = onResume;
        }

        public override void OnEntry()
        {
            _onEntry?.Invoke();
        }

        public override PdStateEventHandle OnTick()
        {
            if (_onTick == null)
            {
                return PdStateEvent.Continue();
            }

            return _onTick.Invoke();
        }

        public override void OnExit()
        {
            _onExit?.Invoke();
        }

        public override void OnPause()
        {
            _onPause?.Invoke();
        }

        public override void OnResume()
        {
            _onResume?.Invoke();
        }
    }
}