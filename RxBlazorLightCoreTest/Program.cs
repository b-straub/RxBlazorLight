// See https://aka.ms/new-console-template for more information
using RxBlazorLightCore;
using RxBlazorLightCoreTest;
using R3;

TestService testService = new();

var done = false;

testService.AsObservable.Subscribe(cr =>
{
    Console.WriteLine(cr.StateID);
    Console.WriteLine(testService.Counter.Value);
    Console.WriteLine(testService.CounterCommandResult);
    Console.WriteLine(testService.StringList.FirstOrDefault());
    Console.WriteLine(testService.NullString);

    if (testService.CommandAsync.Phase is StatePhase.CHANGING)
    {
        testService.CommandAsync.Cancel();
    }

    if (testService.CommandAsync.Phase is StatePhase.CANCELED)
    {
        done = true;
    }
});

testService.IncrementState(testService.Counter);

testService.Counter.Value = 1;
testService.Counter.Value = 2;

testService.Command.Execute(testService.Increment);
testService.Command.Execute(testService.Increment);

testService.Command.Execute(testService.Add(10));
testService.StringListCommand.Execute(testService.AddString("Test"));
testService.StringCommand.Execute(testService.SetString("TestNotNull"));

await testService.CommandAsync.ExecuteAsync(testService.AddAsync(10));

await testService.CommandAsync.ExecuteAsync(testService.IncrementAsync);

await testService.CommandAsync.ExecuteAsync(testService.AddAsyncCancel(10));

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