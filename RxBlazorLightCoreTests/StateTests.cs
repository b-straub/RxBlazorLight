using RxBlazorLightCore;
using RxBlazorLightCoreTestBase;
using Xunit.Abstractions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RxBlazorLightCoreTests
{
    public class StateTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void TestIntState()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;

            fixture.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.IntState.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.IntState.ID}");

                if (sc.StateID == fixture.IntState.ID)
                {
                    stateChangeCount++;
                }
            });

            fixture.IntState.Value = 0;
            Assert.Equal(0, fixture.IntState.Value);

            fixture.IntState.Value++;
            Assert.Equal(1, fixture.IntState.Value);

            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestIncrement()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommand.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommand.Phase}, ID {sc.StateID}, VPID {fixture.IntCommand.ID}");

                if (fixture.IntCommand.Done())
                {
                    done = true;
                }
            });

            fixture.IntCommand.Change(fixture.Reset);
            while (!done) ;
            Assert.False(fixture.CanChangeNotZero());
            Assert.Equal(0, fixture.IntStateResult);

            fixture.IntCommand.Change(fixture.Increment);
            while (!done) ;
            Assert.True(fixture.CanChangeNotZero());

            Assert.Equal(1, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestNotNotify()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommandAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.ID}");

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            await fixture.IntCommandAsync.ChangeAsync(fixture.ResetAsync, false);
            Assert.Equal(0, fixture.IntStateResult);

            await fixture.IntCommandAsync.ChangeAsync(_ => throw new ArgumentOutOfRangeException("Test"), false);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);

            Assert.Equal(2, stateChangeCount);

            await fixture.IntCommandAsync.ChangeAsync(async () => { await Task.Delay(10); fixture.IntCommandAsync.NotifyChanging() ; fixture.IntStateResult = 1; }, false);
            Assert.Equal(1, fixture.IntStateResult);

            await fixture.IntCommandAsync.ChangeAsync(async () => { await Task.Delay(10); throw new ArgumentOutOfRangeException("Test"); }, false);
            while (!done) ;
            Assert.Equal(1, fixture.IntStateResult);

            Assert.Equal(5, stateChangeCount);
        }

        [Fact]
        public void TestAdd()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommand.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommand.Phase}, ID {sc.StateID}, VPID {fixture.IntCommand.ID}");

                if (fixture.IntCommand.Done())
                {
                    done = true;
                }
            });

            fixture.IntCommand.Change(fixture.Reset);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);

            fixture.IntCommand.Change(fixture.Add(10));
            while (!done) ;

            Assert.Equal(10, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestIntStateAsyncX()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommandAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.ID}");

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            fixture.IntStateResult = 10;

            Assert.Equal(10, fixture.IntStateResult);
            Assert.True(fixture.CanChangeBelow(20)());

            await fixture.IntCommandAsync.ChangeAsync(fixture.MultiplyAsync(5));
            while (!done) ;

            Assert.False(fixture.CanChangeBelow(20)());
            Assert.Equal(50, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestAddThrow()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool exception = false;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (!done && sc.StateID == fixture.IntCommandAsync.ID)
                {
                    stateChangeCount++;
                }

                if (fixture.IntCommandAsync.Phase is StatePhase.EXCEPTION &&
                    fixture.Exceptions.First().Exception.Message == "AddAsync")
                {
                    exception = true;
                }

                _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.ID}");

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            fixture.ResetExceptions();
            await fixture.IntCommandAsync.ChangeAsync(fixture.ResetAsync);

            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);

            await fixture.IntCommandAsync.ChangeAsync(fixture.AddAsync(10));
            while (!done) ;
            Assert.Equal(10, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);

            done = false;
            stateChangeCount = 0;

            await fixture.IntCommandAsync.ChangeAsync(fixture.AddAsync(1));
            while (!done) ;

            Assert.True(exception);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestStateObservable()
        {
            ServiceFixture fixture = new();
            var phaseChangeCount = 0;
            bool completed = false;
            bool initialized = false;

            fixture.Subscribe(p =>
            {
                phaseChangeCount++;

                _output.WriteLine($"Value {fixture.IntStateResult}, PCC {phaseChangeCount} Phase {p}");

                if (fixture.IntCommand.Phase is StatePhase.CHANGED)
                {
                    initialized = fixture.IntStateResult == 0;
                    completed = !completed && fixture.IntStateResult == 2;
                }
            });

            fixture.IntCommand.Change(fixture.Reset);
            while (!initialized) ;
            Assert.Equal(0, fixture.IntStateResult);

            var changer = Observable.Range(0, 3);

            var disposable = changer
                .ObserveOn(ImmediateScheduler.Instance)
                .SubscribeOn(ImmediateScheduler.Instance)
                .Subscribe(r => fixture.IntCommand.Change(() => fixture.IntStateResult = r));

            while (!completed) ;

            Assert.Equal(4, phaseChangeCount);
        }

        [Fact]
        public async Task TestCRUDList()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            var done = false;

            IDisposable subscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CRUDListCommand.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDListCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDListCommand.Phase}, ID {sc.StateID}, VPID {fixture.CRUDListCommand.ID}");

                    if (fixture.CRUDListCommand.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDList);
            disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.ADD, new CRUDTest("Item1", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDList);
            Assert.Equal("Item1", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.ADD, new CRUDTest("Item2", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDList.Count);
            Assert.Equal("Item2", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDList.Last();
            var updateItem = lastItem with { Item = "Item3" };

            disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.UPDATE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDList.Count);
            Assert.Equal("Item3", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.DELETE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDList);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListCommand.ChangeAsync(fixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Empty(fixture.CRUDList);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestCRUDDictCommand()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            var done = false;

            IDisposable subscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CRUDDictCommand.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDDictCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDDictCommand.Phase}, ID {sc.StateID}, VPID {fixture.CRUDDictCommand.ID}");

                    if (fixture.CRUDDictCommand.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = subscribeTest();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.CLEAR, default, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDDict);
            disposable = subscribeTest();
            var addGuid1 = Guid.NewGuid();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.ADD, addGuid1, new CRUDTest("Item1", addGuid1))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDict);
            Assert.Equal("Item1", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            var addGuid2 = Guid.NewGuid();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.ADD, addGuid2, new CRUDTest("Item2", addGuid2))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDict.Count());
            Assert.Equal("Item2", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDDict.Last().Value;
            var updateItem = lastItem with { Item = "Item3" };

            disposable = subscribeTest();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.UPDATE, updateItem.Id, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDict.Count());
            Assert.Equal("Item3", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.DELETE, updateItem.Id, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDict);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDDictCommand.ChangeAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.CLEAR, default, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Empty(fixture.CRUDDict);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestStateGroup()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroup.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroup.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroup.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroup.ID}");

                if (fixture.EnumStateGroup.Done())
                {
                    done = true;
                }
            });

            fixture.EnumStateGroup.Change(() => fixture.EnumStateGroup.Value = TestEnum.THREE);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);

            Assert.True(fixture.EnumStateGroup.ItemDisabled(1));

            Assert.Equal(1, stateChangeCount);
        }

        [Fact]
        public void TestStateGroupSync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroup.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroup.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroup.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroup.ID}");

                if (fixture.EnumStateGroup.Done())
                {
                    done = true;
                }
            });

            fixture.EnumStateGroup.Change(() => fixture.EnumStateGroup.Value = TestEnum.THREE);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);

            Assert.True(fixture.EnumStateGroup.ItemDisabled(1));

            Assert.Equal(1, stateChangeCount);
        }

        [Fact]
        public async Task TestStateGroupAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroupAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroupAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroupAsync.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroupAsync.ID}");

                if (fixture.EnumStateGroupAsync.Done())
                {
                    done = true;
                }
            });

            await fixture.EnumStateGroupAsync.ChangeAsync(async ct => 
                { 
                    await Task.Delay(1000, ct); fixture.EnumStateGroupAsync.Value = TestEnum.THREE; 
                });

            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroupAsync.Value);

            Assert.True(fixture.EnumStateGroupAsync.ItemDisabled(1));

            Assert.Equal(2, stateChangeCount);
        }
    }
}