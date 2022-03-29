using System;

namespace PdStateMachine
{
    public static class StateExtensions
    {
        public static PdState WithCondition(this PdState state, Func<bool> condition)
        {
            return new ConditionState(state, condition);
        }
    }
}