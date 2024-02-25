using RxBlazorLightCore;
using RxBlazorLightCoreTestBase;
using Xunit.Abstractions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
                if (sc.ID == fixture.Increment.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.Increment.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Increment.Phase}, ID {sc.ID}, VPID {fixture.Increment.ID}");

                if (fixture.Increment.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.IntState.Transform(0);

            Assert.Equal(0, fixture.IntState.Value);

            fixture.Increment.Provide();
            while (!done) ;

            Assert.Equal(1, fixture.IntState.Value);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TestAdd()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.Add.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.Add.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Add.Phase}, ID {sc.ID}, VPID {fixture.Add.ID}");

                if (fixture.Add.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.IntState.Transform(0);

            Assert.Equal(0, fixture.IntState.Value);

            fixture.Add.Transform(10);
            while (!done) ;

            Assert.Equal(10, fixture.IntState.Value);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TestIntStateAsyncX()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.IntStateAsyncX.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntStateAsyncX.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntStateAsyncX.Phase}, ID {sc.ID}, VPID {fixture.IntStateAsyncX.ID}");

                if (fixture.IntStateAsyncX.Done())
                {
                    done = true;
                }
            }, 0);

            Assert.Equal(10, fixture.IntStateAsyncX.Value);
            fixture.IntStateAsyncX.Transform(5);
            while (!done) ;

            Assert.Equal(50, fixture.IntStateAsyncX.Value);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TestAddThrow()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool exception = false;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (!done && sc.ID == fixture.Add.ID)
                {
                    stateChangeCount++;
                }

                if (fixture.Add.Phase is StateChangePhase.EXCEPTION &&
                    fixture.Exceptions.First().Exception.Message == "AddVP")
                {
                    exception = true;
                }

                _output.WriteLine($"Done {fixture.Add.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Add.Phase}, ID {sc.ID}, VPID {fixture.Add.ID}");

                if (fixture.Add.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ResetExceptions();
            fixture.IntState.Transform(0);

            Assert.Equal(0, fixture.IntState.Value);
            fixture.Add.Transform(10);
            while (!done) ;
            Assert.Equal(10, fixture.IntState.Value);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

            done = false;
            stateChangeCount = 0;

            fixture.Add.Transform(1);
            while (!done) ;

            Assert.True(exception);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TestState()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.TransformTestAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.TransformTestAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.TransformTestAsync.Phase}, ID {sc.ID}, VPID {fixture.TransformTestAsync.ID}");

                if (fixture.TransformTestAsync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.TransformTestAsync.Transform("Async");
            while (!done) ;

            Assert.Equal("Async", fixture.Test);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TransformStateSync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.TransformTestSync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.TransformTestSync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.TransformTestSync.Phase}, ID {sc.ID}, VPID {fixture.TransformTestSync.ID}");

                if (fixture.TransformTestSync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.TransformTestSync.Transform("Sync");
            while (!done) ;

            Assert.Equal("Sync", fixture.Test);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 1);
        }

        [Fact]
        public void TransformStateAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.TransformTestAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.TransformTestAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.TransformTestAsync.Phase}, ID {sc.ID}, VPID {fixture.TransformTestAsync.ID}");

                if (fixture.TransformTestAsync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.TransformTestAsync.Transform("Async");
            while (!done) ;

            Assert.Equal("Async", fixture.Test);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void ProvideStateSync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ProvideTestSync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.ProvideTestSync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ProvideTestSync.Phase}, ID {sc.ID}, VPID {fixture.ProvideTestSync.ID}");

                if (fixture.ProvideTestSync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ProvideTestSync.Provide();
            while (!done) ;

            Assert.Equal("Sync", fixture.Test);
            Assert.Equal(1, stateChangeCount);
        }

        [Fact]
        public void ProvideStateAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ProvideTestAsync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.ProvideTestAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ProvideTestAsync.Phase}, ID {sc.ID}, VPID {fixture.ProvideTestAsync.ID}");

                if (fixture.ProvideTestAsync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ProvideTestAsync.Provide();
            while (!done) ;

            Assert.Equal("Async", fixture.Test);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestStateCancel()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool canceled = false;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.TransformTestAsync.ID)
                {
                    stateChangeCount++;

                    if (fixture.TransformTestAsync.Canceled())
                    {
                        canceled = true;
                    }

                    _output.WriteLine($"Done {fixture.TransformTestAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.TransformTestAsync.Phase}, ID {sc.ID}, VPID {fixture.TransformTestAsync.ID}");

                    if (fixture.TransformTestAsync.Done())
                    {
                        done = true;
                    }
                }
            }, 0);

            fixture.ClearTest();
            Assert.Equal(string.Empty, fixture.Test);

            fixture.TransformTestAsync.Transform("Async");
            fixture.TransformTestAsync.Cancel();

            while (!done) ;

            Assert.Equal(string.Empty, fixture.Test);
            Assert.True(canceled);
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
        }

        [Fact]
        public void TestStateObservable()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool completed = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ObservableStateVoidProvider.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Completed {fixture.ObservableStateVoidProvider.Completed()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ObservableStateVoidProvider.Phase}, ID {sc.ID}, VPID {fixture.ObservableStateVoidProvider.ID}");

                if (fixture.ObservableStateVoidProvider.Completed())
                {
                    completed = true;
                }
            }, 0);

            var changer = Observable.Range(0, 2);

            var disposable = changer.Select(_ => Unit.Default)
                .ObserveOn(ImmediateScheduler.Instance)
                .SubscribeOn(ImmediateScheduler.Instance)
                .Subscribe(fixture.ObservableStateVoidProvider);

            while (!completed) ;

            Assert.Equal(3, stateChangeCount);
        }

        [Fact]
        public void TestStateObservableException()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool exception = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ObservableStateVoidProvider.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Exception {fixture.ObservableStateVoidProvider.Exception()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ObservableStateVoidProvider.Phase}, ID {sc.ID}, VPID {fixture.ObservableStateVoidProvider.ID}");

                if (fixture.ObservableStateVoidProvider.Exception())
                {
                    exception = true;
                }
            }, 0);

            fixture.ObservableStateVoidProvider.OnNext(Unit.Default);

            var changer = Observable.Throw<Unit>(new InvalidOperationException("Test"));

            var disposable = changer
                .ObserveOn(ImmediateScheduler.Instance)
                .SubscribeOn(ImmediateScheduler.Instance)
                .Select(_ => Unit.Default).Subscribe(fixture.ObservableStateVoidProvider);

            while (!exception) ;

            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestStateIntObservable()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool completed = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ObservableStateIntProvider.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Completed {fixture.ObservableStateIntProvider.Completed()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ObservableStateIntProvider.Phase}, ID {sc.ID}, VPID {fixture.ObservableStateIntProvider.ID}");

                if (fixture.ObservableStateIntProvider.Completed())
                {
                    completed = true;
                }
            }, 0);

            var changer = Observable.Range(0, 3);
            var disposable = changer.Subscribe(fixture.ObservableStateIntProvider);

            while (!completed) ;

            Assert.Equal(2, fixture.ObservableIntState.Value);
            Assert.Equal(4, stateChangeCount);
        }

        [Fact]
        public void TestCRUDList()
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
                    if (!done && sc.ID == fixture.CRUDListCmds.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDListCmds.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDListCmds.Phase}, ID {sc.ID}, VPID {fixture.CRUDListCmds.ID}");

                    if (fixture.CRUDListCmds.Done())
                    {
                        done = true;
                    }
                }, 0);
            }

            var disposable = subscribeTest();
            fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.CLEAR, null));
            while (!done) ;
            disposable.Dispose();

            Assert.True(fixture.CRUDListState.HasValue());
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
            disposable.Dispose();

            if (fixture.CRUDListState.HasValue())
            {
                Assert.Empty(fixture.CRUDListState.Value);
                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.ADD, new CRUDTest("Item1", Guid.NewGuid())));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDListState.Value);
                Assert.Equal("Item1", fixture.CRUDListState.Value.Last().Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.ADD, new CRUDTest("Item2", Guid.NewGuid())));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item2", fixture.CRUDListState.Value.Last().Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                var lastItem = fixture.CRUDListState.Value.Last();
                var updateItem = lastItem with { Item = "Item3" };

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.UPDATE, updateItem));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item3", fixture.CRUDListState.Value.Last().Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.DELETE, updateItem));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDListState.Value);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD_LIST.CLEAR, null));
                while (!done) ;
                disposable.Dispose();

                Assert.Empty(fixture.CRUDListState.Value);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
            }
        }

        [Fact]
        public void TestCRUDDict()
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
                    if (!done && sc.ID == fixture.CRUDDictCmds.ID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDDictCmds.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDDictCmds.Phase}, ID {sc.ID}, VPID {fixture.CRUDDictCmds.ID}");

                    if (fixture.CRUDDictCmds.Done())
                    {
                        done = true;
                    }
                }, 0);
            }

            var disposable = subscribeTest();
            fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.CLEAR, default, null));
            while (!done) ;
            disposable.Dispose();

            Assert.True(fixture.CRUDDictState.HasValue());
            Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
            disposable.Dispose();

            if (fixture.CRUDDictState.HasValue())
            {
                Assert.Empty(fixture.CRUDDictState.Value);
                disposable = subscribeTest();
                var addGuid1 = Guid.NewGuid();
                fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.ADD, addGuid1, new CRUDTest("Item1", addGuid1)));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDDictState.Value);
                Assert.Equal("Item1", fixture.CRUDDictState.Value.Last().Value.Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                var addGuid2 = Guid.NewGuid();
                fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.ADD, addGuid2, new CRUDTest("Item2", addGuid2)));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDDictState.Value.Count());
                Assert.Equal("Item2", fixture.CRUDDictState.Value.Last().Value.Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                var lastItem = fixture.CRUDDictState.Value.Last().Value;
                var updateItem = lastItem with { Item = "Item3" };

                disposable = subscribeTest();
                fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.UPDATE, updateItem.Id, updateItem));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDDictState.Value.Count());
                Assert.Equal("Item3", fixture.CRUDDictState.Value.Last().Value.Item);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.DELETE, updateItem.Id, null));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDDictState.Value);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);

                disposable = subscribeTest();
                fixture.CRUDDictCmds.Transform((ServiceFixture.IntDictVP.CMD_DICT.CLEAR, default, null));
                while (!done) ;
                disposable.Dispose();

                Assert.Empty(fixture.CRUDDictState.Value);
                Assert.True(stateChangeCount > 0 && stateChangeCount <= 2);
            }
        }
    }
}