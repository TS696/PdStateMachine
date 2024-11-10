using NUnit.Framework;
using PdStateMachine;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class Test
    {
        [TestCase(true)]
        [TestCase(false)]
        public void PushState(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            stateStack.PushState(new TestState("TestState"));
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PushStates(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

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

        [TestCase(true)]
        [TestCase(false)]
        public void PushStateTwice(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

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

        [TestCase(true)]
        [TestCase(false)]
        public void PushSubState(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

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

        [TestCase(true)]
        [TestCase(false)]
        public void PushSubStates(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

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

        [TestCase(true)]
        [TestCase(false)]
        public void PushRegisteredState(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            var state = new TestState("TestState");

            stateStack.RegisterState<TestState>(state);
            stateStack.PushState<TestState>();

            stateStack.Tick();

            stateStack.Dispose();
            LogAssert.Expect(LogType.Log, "TestState Entry");
            LogAssert.Expect(LogType.Log, "TestState Tick");
            LogAssert.Expect(LogType.Log, "TestState Exit");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PushRegisteredStates(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

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

        [TestCase(true)]
        [TestCase(false)]
        public void PushRegisteredSubState(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            var isPushed = false;
            var stateA = new TestState("TestStateA", () =>
            {
                if (isPushed)
                {
                    return PdStateEvent.Pop();
                }

                isPushed = true;
                return PdStateEvent.PushSubState<TestState>();
            });
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

        [TestCase(true)]
        [TestCase(false)]
        public void RaiseMessageWithNoHandle(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            var stateA = new TestState("TestStateA");
            var stateB = new TestState("TestStateB");
            stateStack.PushState(stateA);
            stateStack.PushState(stateB);
            stateStack.UnhandledMessage += (_, _) =>
            {
                Debug.Log("UnhandledMessage");
            };

            stateStack.RaiseMessage(new TestStateMessage());
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateB HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateA HandleMessage");
            LogAssert.Expect(LogType.Log, "UnhandledMessage");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RaiseMessageWithHandle(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            var stateA = new TestState("TestStateA");
            var stateB = new TestState("TestStateB", null, m =>
            {
                Debug.Log(m.Message);
                return true;
            });
            var stateC = new TestState("TestStateC");

            stateStack.PushState(stateA);
            stateStack.PushState(stateB);
            stateStack.PushState(stateC);
            stateStack.UnhandledMessage += (_, _) =>
            {
                Assert.Fail();
            };

            stateStack.RaiseMessage(new TestStateMessage() { Message = "Handle" });
            stateStack.Tick();
            stateStack.Dispose();

            LogAssert.Expect(LogType.Log, "TestStateC HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateB HandleMessage");
            LogAssert.Expect(LogType.Log, "Handle");
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RaiseMessageFromState(bool tickUntilContinue)
        {
            var stateStack = new PdStateStack();
            stateStack.TickUntilContinue = tickUntilContinue;

            var stateA = new TestState("TestStateA");
            var stateB = new TestState("TestStateB", () => PdStateEvent.RaiseMessage(new TestStateMessage()));
            stateStack.PushState(stateA);
            stateStack.PushState(stateB);

            stateStack.Tick();
            stateStack.Tick();

            stateStack.Dispose();
            
            LogAssert.Expect(LogType.Log, "TestStateB Entry");
            LogAssert.Expect(LogType.Log, "TestStateB Tick");
            LogAssert.Expect(LogType.Log, "TestStateB HandleMessage");
            LogAssert.Expect(LogType.Log, "TestStateB Exit");
            LogAssert.Expect(LogType.Log, "TestStateA HandleMessage");
        }

        private class TestStateMessage : IStateMessage
        {
            public object Message;
        }

        private class TestState : PdState, IStateMessageReceiver<TestStateMessage>
        {
            private readonly string _logName;
            private readonly Func<PdStateEventHandle> _onTick;
            private readonly Func<TestStateMessage, bool> _handleMessage;

            private PdStateEventHandle _handleCache;

            public TestState(string logName, Func<PdStateEventHandle> onTick = null, Func<TestStateMessage, bool> handleMessage = null)
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

            public override PdStateEventHandle OnTick()
            {
                Assert.IsNotNull(Context);
                Assert.AreEqual(Context.Status, StateStatus.Active);
                Debug.Log($"{_logName} Tick");
                return _onTick != null ? _onTick.Invoke() : PdStateEvent.Continue();
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

            public bool HandleMessage(TestStateMessage stateMessage, PdState sender)
            {
                Assert.IsNotNull(Context);
                Debug.Log($"{_logName} HandleMessage");
                if (_handleMessage != null)
                {
                    return _handleMessage.Invoke(stateMessage);
                }

                return false;
            }
        }
    }
}
