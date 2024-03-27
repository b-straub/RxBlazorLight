﻿// See https://aka.ms/new-console-template for more information
using RxBlazorLightCore;
using RxBlazorLightCoreTest;
using System.Reactive.Linq;

var done = false;

TestService testService = new();

testService.Subscribe(cr =>
{
    Console.WriteLine(cr.StateID);
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

testService.Counter.Value = 1;
testService.Counter.Value = 2;

testService.CounterCommand.Change(testService.Increment);
testService.CounterCommand.Change(testService.Increment);

testService.CounterCommand.Change(testService.Add(10));
testService.StringListCommand.Change(testService.AddString("Test"));
testService.StringCommand.Change(testService.SetString("TestNotNull"));

await testService.CounterCommandAsync.ChangeAsync(testService.AddAsync(10));

await testService.CounterCommandAsync.ChangeAsync(testService.IncrementAsync);

await testService.CounterCommandAsync.ChangeAsync(testService.AddAsyncCancel(10));

var ccCounter = testService.CanChangeT(1);
ccCounter = testService.CanChangeT(100);
var ccCounterAsync = testService.CanChangeNV;
var ccNullString = testService.CanChangeS("TestNotNull");

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