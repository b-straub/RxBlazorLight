// See https://aka.ms/new-console-template for more information
using RxBlazorLightCore;
using RxBlazorLightCoreTest;
using RxBlazorLightCoreTestBase;
using System.Reactive.Linq;

var done = false;

TestService testService = new();

var obs = testService.CreateObservableStateAsync(async (s, ct) =>
{
    await Task.Delay(1000, ct);
    s.Counter += 2;
});

obs.Subscribe(p =>
{
    Console.WriteLine(p.ToString());
    if (p is StateChangePhase.CHANGING)
    {
        obs.Cancel();
    }
});

testService.Subscribe(cr =>
{
    if (testService.State.Counter == 24)
    {
        done = true;
    }
});

await obs.SetAsync();

testService.SetState(TestServiceState.Increment);

testService.SetState(TestServiceState.AddDirect(10));

await testService.SetStateAsync(TestServiceState.IncrementAsync, p =>
{
    Console.WriteLine(p.ToString());
});

await testService.SetStateAsync(TestServiceState.AddAsync);

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