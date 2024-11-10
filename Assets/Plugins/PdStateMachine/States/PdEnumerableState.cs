using System.Collections.Generic;

namespace PdStateMachine
{
    public abstract class PdEnumerableState : PdState
    {
        private IEnumerator<PdStateEventHandle> _enumerator;

        public override void OnEntry()
        {
            _enumerator = Execute();
        }

        public override PdStateEventHandle OnTick()
        {
            if (_enumerator.MoveNext())
            {
                return _enumerator.Current;
            }

            return PdStateEvent.Pop();
        }

        protected abstract IEnumerator<PdStateEventHandle> Execute();
    }
}