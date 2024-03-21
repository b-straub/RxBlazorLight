// See https://aka.ms/new-console-template for more information
using RxBlazorLightCore;
using RxBlazorLightCoreTest;
using System.Reactive.Linq;

var done = false;

TestService testService = new();

testService.CounterAsync.Subscribe(p =>
{
    Console.WriteLine(p.ToString());
    if (p is StatePhase.CHANGING)
    {
        testService.CounterAsync.Cancel();
    }
});

testService.Subscribe(cr =>
{
    Console.WriteLine(cr.StateID);

    if (testService.CounterAsync.Phase is StatePhase.CANCELED)
    {
        done = true;
    }
});

testService.Counter.Change(TestService.Increment);
testService.Counter.Change(TestService.Increment);

testService.Counter.Change(TestService.Add(10));
testService.StringList.Change(TestService.AddString("Test"));
testService.NullString.Change(s => s.Value = "TestNotNull");

await testService.CounterAsync.ChangeAsync(TestService.AddAsync(10));

await testService.CounterAsync.ChangeAsync(TestService.IncrementAsync);

await testService.CounterAsync.ChangeAsync(TestService.AddAsyncLR(10));

var ccCounter = testService.Counter.CanChange(TestService.CanChangeT(1));
ccCounter = testService.Counter.CanChange(TestService.CanChangeT(100));
var ccCounterAsync = testService.CounterAsync.CanChange(TestService.CanChangeNV);
var ccNullString = testService.NullString.CanChange(TestService.CanChangeS("TestNotNull"));

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

while (!done) { };

Environment.Exit(0);