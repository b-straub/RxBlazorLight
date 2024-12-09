// See https://aka.ms/new-console-template for more information
using RxBlazorLightCore;
using RxBlazorLightCoreTest;
using R3;

TestService testService = new();

var done = false;

testService.AsObservable.Subscribe(cr =>
{
    Console.WriteLine(cr.ID);
    Console.WriteLine(testService.Counter.Value);
    Console.WriteLine(testService.CounterCommandResult);
    Console.WriteLine(testService.StringList.FirstOrDefault());
    Console.WriteLine(testService.NullString);

    if (testService.CounterCommandAsync.Phase is StatePhase.CHANGING)
    {
        testService.CounterCommandAsync.Cancel();
    }

    if (testService.CounterCommandAsync.Phase is StatePhase.CANCELED)
    {
        done = true;
    }
});

testService.IncrementState(testService.Counter);

testService.Counter.Value = 1;
testService.Counter.Value = 2;

testService.CounterCommand.Execute(testService.Increment);
testService.CounterCommand.Execute(testService.Increment);

testService.CounterCommand.Execute(testService.Add(10));
testService.StringListCommand.Execute(testService.AddString("Test"));
testService.StringCommand.Execute(testService.SetString("TestNotNull"));

await testService.CounterCommandAsync.ExecuteAsync(testService.AddAsync(10));

await testService.CounterCommandAsync.ExecuteAsync(testService.IncrementAsync);

await testService.CounterCommandAsync.ExecuteAsync(testService.AddAsyncCancel(10));

testService.CanChangeT(1);
testService.CanChangeT(100);
testService.CanChangeS("TestNotNull");

/*ServiceFixture fixture = new();

var done = false;

fixture.Subscribe(cr =>
{
    if (fixture.IntStateAsyncX.Done())
    {
        done = true;
    }
});

fixture.IntStateAsyncX.Transform(5);*/

while (!done) { }

Environment.Exit(0);