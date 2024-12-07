# PdStateMachine
Pushdown State Machine library for unity.

## Installation
Add the following line directly to Packages/manifest.json:
```json
"com.ts696.pdstatemachine": "https://github.com/TS696/PdStateMachine.git?path=Assets/Plugins/PdStateMachine#2.0.1"
```

## Basic Usage
First, define a state as follows:
```csharp
private class SampleState : PdState
{
    private readonly int _tickCount;
    private int _current;

    public SampleState(int tickCount)
    {
        _tickCount = tickCount;
    }

    public override void OnEntry()
    {
        Debug.Log($"SampleState OnEntry");
        _current = 0;
    }

    public override PdStateEventHandle OnTick()
    {
        _current++;
        Debug.Log($"SampleState OnTick {_current}");

        if (_current < _tickCount)
        {
            return PdStateEvent.Continue();
        }
        
        return PdStateEvent.Pop();
    }

    public override void OnExit()
    {
        Debug.Log($"SampleState OnExit");
    }
}
```

Next, define a PdStateStack, push the state, and execute it.
```csharp
var stateStack = new PdStateStack();
stateStack.PushState(new SampleState(3));

stateStack.Tick();
stateStack.Tick();
stateStack.Tick();
```

The output of the above code will be as follows:
```
SampleState OnEntry
SampleState OnTick 1
SampleState OnTick 2
SampleState OnTick 3
SampleState OnExit
```

Since states are registered using a stack structure, the most recently pushed state is executed first.
```csharp
var stateA = new State();
var stateB = new State();
stateStack.PushState(stateA);
stateStack.PushState(stateB);

stateStack.Tick(); // Executes stateB because it is the most recently pushed state
```
This behavior follows the Last In, First Out (LIFO) principle of a stack. If `stateB` is popped, execution will return to `stateA`.

### Pre-registering and Pushing States by Type
You can pre-register state instances and push them later by specifying their type.
```csharp
var stateStack = new PdStateStack();
stateStack.RegisterState(new SampleState());
stateStack.PushState<SampleState>();
```
Instead of creating a new instance, the stack looks for the pre-registered `SampleState` and pushes it onto the stack. This method avoids redundant instantiation of states.

### Message Propagation
By using message types that implement the `IStateMessage` interface, you can propagate messages through the stack, enabling behavior similar to a global exit.
#### Define a Message
```csharp
private class Message : IStateMessage
{
    public string Log { get; }

    public Message(string log)
    {
        Log = log;
    }
}
```
#### Define States
```csharp
private class RaiseMessageState : PdState
{
    public override PdStateEventHandle OnTick()
    {
        return PdStateEvent.RaiseMessage(new Message("Raise message"));
    }
}

private class HandleMessageState : PdState, IStateMessageReceiver<Message>
{
    public override PdStateEventHandle OnTick()
    {
        Debug.Log("HandleMessageState Tick");
        return PdStateEvent.Pop();
    }

    public bool HandleMessage(Message message, PdState sender)
    {
        Debug.Log(message.Log);
        return true; // Stops message propagation
    }
}
```
#### Execute
```csharp
var stateStack = new PdStateStack();

stateStack.PushState(new HandleMessageState());
stateStack.PushState(new DummyState());
stateStack.PushState(new RaiseMessageState());

stateStack.Tick(); // Output: "Raise message"
stateStack.Tick(); // Output: "HandleMessageState Tick"
```

##### First Tick
- **RaiseMessageState** calls `PdStateEvent.RaiseMessage()`.  
- The message propagates through **DummyState** and **HandleMessageState**.  
- **HandleMessageState** calls `HandleMessage()`, which logs **"Raise message"** and returns `true` to stop message propagation.  
- The stack pops **DummyState** and **RaiseMessageState**, so **HandleMessageState** becomes the active state.  
##### Second Tick
- The current state is now **HandleMessageState**, so its `OnTick()` method is executed, and "HandleMessageState Tick" is logged.

## License
This library is released under the MIT License.

