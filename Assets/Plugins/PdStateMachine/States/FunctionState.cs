using System;

namespace PdStateMachine
{
    public class FunctionState : PdState
    {
        private readonly Action _onEntry;
        private readonly Func<PdStateEvent> _onTick;
        private readonly Action _onExit;
        private readonly Action _onPause;
        private readonly Action _onResume;
        private readonly Func<object, bool> _handleMessage;

        public FunctionState(Action onEntry = null, Func<PdStateEvent> onTick = null, Action onExit = null,
            Action onPause = null, Action onResume = null, Func<object, bool> handleMessage = null)
        {
            _onEntry = onEntry;
            _onTick = onTick;
            _onExit = onExit;
            _onPause = onPause;
            _onResume = onResume;
            _handleMessage = handleMessage;
        }

        public override void OnEntry()
        {
            _onEntry?.Invoke();
        }

        public override PdStateEvent OnTick()
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

        public override bool HandleMessage(object message)
        {
            if (_handleMessage == null)
            {
                return false;
            }

            return _handleMessage.Invoke(message);
        }
    }
}