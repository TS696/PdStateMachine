using System;

namespace PdStateMachine
{
    public class ConditionState : PdState
    {
        private readonly Func<bool> _condition;
        private readonly PdState _state;

        public ConditionState(PdState state, Func<bool> condition)
        {
            _state = state;
            _condition = condition;
        }

        public override void OnEntry()
        {
            _state.OnEntry();
        }

        public override PdStateEvent OnTick()
        {
            if (!_condition.Invoke())
            {
                return PdStateEvent.Pop();
            }

            return _state.OnTick();
        }

        public override void OnExit()
        {
            _state.OnExit();
        }

        public override void OnPause()
        {
            _state.OnPause();
        }

        public override void OnResume()
        {
            _state.OnResume();
        }
    }
}