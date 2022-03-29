using NUnit.Framework;
using PdStateMachine;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Test
    {
        [Test]
        public void PushState()
        {
            var stateStack = new PdStateStack();
            stateStack.PushState(new TestState("TestState"));
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
        }

        [Test]
        public void PushStates()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA", PdStateEvent.Pop);
            var stateB = new TestState("TestStateB");
            stateStack.PushStates(stateA, stateB);
            stateStack.Tick();
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateA Entry");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Exit");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
        }

        [Test]
        public void PushStateTwice()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA");
            stateStack.PushState(stateA);
            stateStack.Tick();

            var stateB = new TestState("TestStateB", PdStateEvent.Pop);
            stateStack.PushState(stateB);
            stateStack.Tick();
            stateStack.Tick();

            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateA Entry");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Pause");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
            LogAssert.Expect(LogType.Log, "TestStateA Resume");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Exit");
        }

        [Test]
        public void PushSubState()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA", PdStateEvent.Pop);
            var isPushed = false;
            var stateB = new TestState("TestStateB", () =>
            {
                if (isPushed)
                {
                    return PdStateEvent.Continue();
                }

                isPushed = true;
                return PdStateEvent.PushSubState(stateA);
            });

            stateStack.PushState(stateB);
            stateStack.Tick();
            stateStack.Tick();
            stateStack.Tick();

            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Pause");
            LogAssert.Expect(LogType.Log, "TestStateA Entry");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Exit");
            LogAssert.Expect(LogType.Log, "TestStateB Resume");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
        }

        [Test]
        public void PushSubStates()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA", PdStateEvent.Pop);
            var stateB = new TestState("TestStateB", PdStateEvent.Pop);
            var isPushed = false;
            var stateC = new TestState("TestStateC", () =>
            {
                if (isPushed)
                {
                    return PdStateEvent.Continue();
                }

                isPushed = true;
                return PdStateEvent.PushSubStates(new PdState[] { stateA, stateB });
            });

            stateStack.PushState(stateC);
            stateStack.Tick();
            stateStack.Tick();
            stateStack.Tick();
            stateStack.Tick();

            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateC Entry");
            LogAssert.Expect(LogType.Log, "TestStateC Tick");
            LogAssert.Expect(LogType.Log, "TestStateC Pause");
            LogAssert.Expect(LogType.Log, "TestStateA Entry");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Exit");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
            LogAssert.Expect(LogType.Log, "TestStateC Resume");
            LogAssert.Expect(LogType.Log, "TestStateC Tick");
            LogAssert.Expect(LogType.Log, "TestStateC Exit");
        }


        private class TestState : PdState
        {
            private readonly string _logName;
            private readonly Func<PdStateEvent> _onTick;

            public TestState(string logName, Func<PdStateEvent> onTick = null)
            {
                _logName = logName;
                _onTick = onTick;
            }

            public override void OnEntry()
            {
                Debug.Log($"{_logName} Entry");
            }

            public override PdStateEvent OnTick()
            {
                Debug.Log($"{_logName} Tick");
                return _onTick?.Invoke();
            }

            public override void OnExit()
            {
                Debug.Log($"{_logName} Exit");
            }

            public override void OnPause()
            {
                Debug.Log($"{_logName} Pause");
            }

            public override void OnResume()
            {
                Debug.Log($"{_logName} Resume");
            }
        }
    }
}