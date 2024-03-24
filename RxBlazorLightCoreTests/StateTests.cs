using RxBlazorLightCore;
using RxBlazorLightCoreTestBase;
using Xunit.Abstractions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Reactive;

namespace RxBlazorLightCoreTests
{
    public class StateTests(ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void TestIncrement()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntState.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntState.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntState.Phase}, ID {sc.StateID}, VPID {fixture.IntState.ID}");

                if (fixture.IntState.Done())
                {
                    done = true;
                }
            });

            fixture.IntState.Change(s => s.Value = 0);
            while (!done) ;
            Assert.False(fixture.IntState.CanChange(ServiceFixture.CanChangeNotZero));
            Assert.Equal(0, fixture.IntState.Value);

            fixture.IntState.Change(ServiceFixture.Increment);
            while (!done) ;
            Assert.True(fixture.IntState.CanChange(ServiceFixture.CanChangeNotZero));

            Assert.Equal(1, fixture.IntState.Value);
            Assert.Equal(4, stateChangeCount);
        }

        [Fact]
        public async Task TestNotNotify()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntStateAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntStateAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntStateAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntStateAsync.ID}");

                if (fixture.IntStateAsync.Done())
                {
                    done = true;
                }
            });

            await fixture.IntStateAsync.ChangeAsync(s => { s.Value = 0; return Task.CompletedTask; }, false);
            Assert.Equal(0, fixture.IntStateAsync.Value);

            await fixture.IntStateAsync.ChangeAsync(_ => throw new ArgumentOutOfRangeException("Test"), false);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateAsync.Value);

            Assert.Equal(1, stateChangeCount);

            await fixture.IntStateAsync.ChangeAsync(async s => { await Task.Delay(10); s.NotifyChanging() ; s.Value = 1; }, false);
            Assert.Equal(1, fixture.IntStateAsync.Value);

            await fixture.IntStateAsync.ChangeAsync(async s => { await Task.Delay(10); throw new ArgumentOutOfRangeException("Test"); }, false);
            while (!done) ;
            Assert.Equal(1, fixture.IntStateAsync.Value);

            Assert.Equal(4, stateChangeCount);
        }

        [Fact]
        public void TestAdd()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntState.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntState.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntState.Phase}, ID {sc.StateID}, VPID {fixture.IntState.ID}");

                if (fixture.IntState.Done())
                {
                    done = true;
                }
            });

            fixture.IntState.Change(s => s.Value = 0);
            while (!done) ;
            Assert.Equal(0, fixture.IntState.Value);

            fixture.IntState.Change(ServiceFixture.Add(10));
            while (!done) ;

            Assert.Equal(10, fixture.IntState.Value);
            Assert.Equal(4, stateChangeCount);
        }

        [Fact]
        public async Task TestIntStateAsyncX()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntStateAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntStateAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntStateAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntStateAsync.ID}");

                if (fixture.IntStateAsync.Done())
                {
                    done = true;
                }
            });

            Assert.Equal(10, fixture.IntStateAsync.Value);
            Assert.True(fixture.IntStateAsync.CanChange(ServiceFixture.CanChangeBelow(20)));

            await fixture.IntStateAsync.ChangeAsync(ServiceFixture.MultiplyAsync(5));
            while (!done) ;

            Assert.False(fixture.IntStateAsync.CanChange(ServiceFixture.CanChangeBelow(20)));
            Assert.Equal(50, fixture.IntStateAsync.Value);
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
                if (!done && sc.StateID == fixture.IntStateAsync.ID)
                {
                    stateChangeCount++;
                }

                if (fixture.IntStateAsync.Phase is StatePhase.EXCEPTION &&
                    fixture.Exceptions.First().Exception.Message == "AddAsync")
                {
                    exception = true;
                }

                _output.WriteLine($"Done {fixture.IntStateAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntStateAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntStateAsync.ID}");

                if (fixture.IntStateAsync.Done())
                {
                    done = true;
                }
            });

            fixture.ResetExceptions();
            await fixture.IntStateAsync.ChangeAsync(s => { s.Value = 0; return Task.CompletedTask; });

            while (!done) ;
            Assert.Equal(0, fixture.IntStateAsync.Value);

            await fixture.IntStateAsync.ChangeAsync(ServiceFixture.AddAsync(10));
            while (!done) ;
            Assert.Equal(10, fixture.IntStateAsync.Value);
            Assert.Equal(1, stateChangeCount);

            done = false;
            stateChangeCount = 0;

            await fixture.IntStateAsync.ChangeAsync(ServiceFixture.AddAsync(1));
            while (!done) ;

            Assert.True(exception);
            Assert.Equal(1, stateChangeCount);
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

                _output.WriteLine($"Value {fixture.IntState.Value}, PCC {phaseChangeCount} Phase {p}");

                if (fixture.IntState.Phase is StatePhase.CHANGED)
                {
                    initialized = fixture.IntState.Value == 0;
                    completed = !completed && fixture.IntState.Value == 2;
                }
            });

            fixture.IntState.Change(s => s.Value = 0);
            while (!initialized) ;
            Assert.Equal(0, fixture.IntState.Value);

            var changer = Observable.Range(0, 3);

            var disposable = changer
                .ObserveOn(ImmediateScheduler.Instance)
                .SubscribeOn(ImmediateScheduler.Instance)
                .Subscribe(r => fixture.IntState.Change(s => s.Value = r));

            while (!completed) ;

            Assert.Equal(8, phaseChangeCount);
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
                    if (!done && sc.StateID == fixture.CRUDListState.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDListState.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDListState.Phase}, ID {sc.StateID}, VPID {fixture.CRUDListState.ID}");

                    if (fixture.CRUDListState.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDListState.Value);
            disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.ADD, new CRUDTest("Item1", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDListState.Value);
            Assert.Equal("Item1", fixture.CRUDListState.Value.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.ADD, new CRUDTest("Item2", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDListState.Value.Count);
            Assert.Equal("Item2", fixture.CRUDListState.Value.Last().Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDListState.Value.Last();
            var updateItem = lastItem with { Item = "Item3" };

            disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.UPDATE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDListState.Value.Count);
            Assert.Equal("Item3", fixture.CRUDListState.Value.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.DELETE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDListState.Value);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDListState.ChangeAsync(ServiceFixture.ChangeCrudListAsync((ServiceFixture.CMD_CRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Empty(fixture.CRUDListState.Value);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestCRUDDict()
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
                    if (!done && sc.StateID == fixture.CRUDDictState.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDDictState.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDDictState.Phase}, ID {sc.StateID}, VPID {fixture.CRUDDictState.ID}");

                    if (fixture.CRUDDictState.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = subscribeTest();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.CLEAR, default, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDDictState.Value);
            disposable = subscribeTest();
            var addGuid1 = Guid.NewGuid();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.ADD, addGuid1, new CRUDTest("Item1", addGuid1))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDictState.Value);
            Assert.Equal("Item1", fixture.CRUDDictState.Value.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            var addGuid2 = Guid.NewGuid();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.ADD, addGuid2, new CRUDTest("Item2", addGuid2))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDictState.Value.Count());
            Assert.Equal("Item2", fixture.CRUDDictState.Value.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDDictState.Value.Last().Value;
            var updateItem = lastItem with { Item = "Item3" };

            disposable = subscribeTest();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.UPDATE, updateItem.Id, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDictState.Value.Count());
            Assert.Equal("Item3", fixture.CRUDDictState.Value.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.DELETE, updateItem.Id, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDictState.Value);
            Assert.Equal(2, stateChangeCount);

            disposable = subscribeTest();
            await fixture.CRUDDictState.ChangeAsync(ServiceFixture.ChangeCrudDictAsync((ServiceFixture.CMD_CRUD.CLEAR, default, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Empty(fixture.CRUDDictState.Value);
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

            fixture.EnumStateGroup.Change(s => s.Value = TestEnum.THREE);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);

            Assert.True(fixture.EnumStateGroup.ItemDisabled(1));

            Assert.Equal(2, stateChangeCount);
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

            fixture.EnumStateGroup.Change(s =>  s.Value = TestEnum.THREE);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);

            Assert.True(fixture.EnumStateGroup.ItemDisabled(1));

            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestStateGroupAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroupLR.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroupLR.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroupLR.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroupLR.ID}");

                if (fixture.EnumStateGroupLR.Done())
                {
                    done = true;
                }
            });

            await fixture.EnumStateGroupLR.ChangeAsync(async static (s, ct) => { await Task.Delay(1000, ct); s.Value = TestEnum.THREE; });
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroupLR.Value);

            Assert.True(fixture.EnumStateGroupLR.ItemDisabled(1));

            Assert.Equal(2, stateChangeCount);
        }
    }
}