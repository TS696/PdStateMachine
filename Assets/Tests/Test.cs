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

        [Test]
        public void PushRegisteredState()
        {
            var stateStack = new PdStateStack();
            var state = new TestState("TestState");

            stateStack.RegisterState<TestState>(state);
            stateStack.PushState<TestState>();

            stateStack.Tick();

            stateStack.Dispose();
            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
        }

        [Test]
        public void PushRegisteredStates()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestState", PdStateEvent.Pop);
            stateStack.RegisterState(stateA);
            stateStack.PushStates(typeof(TestState), typeof(TestState));
            stateStack.Tick();
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
        }

        [Test]
        public void PushRegisteredSubState()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA", () => PdStateEvent.PushSubState<TestState>());
            var stateB = new TestState("TestStateB", PdStateEvent.Pop);

            stateStack.RegisterState<TestState>(stateB);
            stateStack.PushState(stateA);

            stateStack.Tick();
            stateStack.Tick();

            stateStack.Dispose();
            LogAssert.Expect(LogType.Log, "TestStateA Entry");
            LogAssert.Expect(LogType.Log, "TestStateA Tick");
            LogAssert.Expect(LogType.Log, "TestStateA Pause");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
            LogAssert.Expect(LogType.Log, "TestStateA Exit");
        }

        [Test]
        public void RaiseMessageWithNoHandle()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA");
            var stateB = new TestState("TestStateB");
            stateStack.PushState(stateA);
            stateStack.PushState(stateB);

            stateStack.RaiseMessage(null);
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateB HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateA HandleMessage");
        }

        [Test]
        public void RaiseMessageWithHandle()
        {
            var stateStack = new PdStateStack();
            var stateA = new TestState("TestStateA");
            var stateB = new TestState("TestStateB", null, _ => true);
            var stateC = new TestState("TestStateC");

            stateStack.PushState(stateA);
            stateStack.PushState(stateB);
            stateStack.PushState(stateC);

            stateStack.RaiseMessage(null);
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateC HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateB HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
        }

        private class TestState : PdState
        {
            private readonly string _logName;
            private readonly Func<PdStateEvent> _onTick;
            private readonly Func<object, bool> _handleMessage;

            public TestState(string logName, Func<PdStateEvent> onTick = null, Func<object, bool> handleMessage = null)
            {
                _logName = logName;
                _onTick = onTick;
                _handleMessage = handleMessage;
            }

            public override void OnEntry()
            {
                Assert.IsNotNull(Context);
                Assert.AreEqual(Context.Status, StateStatus.Disable);
                Debug.Log($"{_logName} Entry");
            }

            public override PdStateEvent OnTick()
            {
                Assert.IsNotNull(Context);
                Assert.AreEqual(Context.Status, StateStatus.Active);
                Debug.Log($"{_logName} Tick");
                return _onTick?.Invoke();
            }

            public override void OnExit()
            {
                Assert.IsNotNull(Context);
                Assert.Contains(Context.Status, new[] { StateStatus.Active, StateStatus.Pause });
                Debug.Log($"{_logName} Exit");
            }

            public override void OnPause()
            {
                Assert.IsNotNull(Context);
                Assert.AreEqual(Context.Status, StateStatus.Active);
                Debug.Log($"{_logName} Pause");
            }

            public override void OnResume()
            {
                Assert.IsNotNull(Context);
                Assert.AreEqual(Context.Status, StateStatus.Pause);
                Debug.Log($"{_logName} Resume");
            }

            public override bool HandleMessage(StateMessage message)
            {
                Assert.IsNotNull(Context);
                Debug.Log($"{_logName} HandleMessage");

                if (_handleMessage != null)
                {
                    return _handleMessage.Invoke(message);
                }

                return false;
            }
        }
    }
}