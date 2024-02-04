using RxBlazorLightCore;
using RxBlazorLightCoreTestBase;
using Xunit.Abstractions;

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
            while (!done);

            Assert.Equal(1, fixture.IntState.Value);
            Assert.Equal(2, stateChangeCount);
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
            Assert.Equal(2, stateChangeCount);
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
            Assert.Equal(2, stateChangeCount);
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
            Assert.Equal(2, stateChangeCount);

            done = false;
            stateChangeCount = 0;

            fixture.Add.Transform(1);
            while (!done) ;

            Assert.True(exception);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestState()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ChangeTest.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.ChangeTest.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTest.Phase}, ID {sc.ID}, VPID {fixture.ChangeTest.ID}");

                if (fixture.ChangeTest.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTest.Transform("Test");
            while (!done) ;

            Assert.Equal("Test", fixture.Test);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public void TestStateSync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.Subscribe(sc =>
            {
                if (sc.ID == fixture.ChangeTestSync.ID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.ChangeTestSync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTestSync.Phase}, ID {sc.ID}, VPID {fixture.ChangeTestSync.ID}");

                if (fixture.ChangeTestSync.Done())
                {
                    done = true;
                }
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTestSync.Provide();
            while (!done) ;

            Assert.Equal("Sync", fixture.Test);
            Assert.Equal(1, stateChangeCount);
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
                if (sc.ID == fixture.ChangeTest.ID)
                {
                    stateChangeCount++;

                    if (fixture.ChangeTest.Phase is StateChangePhase.CANCELED)
                    {
                        canceled = true;
                    }

                    _output.WriteLine($"Done {fixture.ChangeTest.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTest.Phase}, ID {sc.ID}, VPID {fixture.ChangeTest.ID}");

                    if (fixture.ChangeTest.Done())
                    {
                        done = true;
                    }
                }
            }, 0);

            fixture.ClearTest();
            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTest.Transform("Test");
            fixture.ChangeTest.Cancel();

            while (!done) ;

            Assert.Equal(string.Empty, fixture.Test);
            Assert.True(canceled);
            Assert.Equal(2, stateChangeCount);
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
            fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.CLEAR, null));
            while (!done) ;
            disposable.Dispose();

            Assert.True(fixture.CRUDListState.HasValue());
            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            if (fixture.CRUDListState.HasValue())
            {
                Assert.Empty(fixture.CRUDListState.Value);
                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.ADD, new CRUDTest("Item1", Guid.NewGuid())));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDListState.Value);
                Assert.Equal("Item1", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.ADD, new CRUDTest("Item2", Guid.NewGuid())));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item2", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                var lastItem = fixture.CRUDListState.Value.Last();
                var updateItem = lastItem with { Item = "Item3" };

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.UPDATE, updateItem));
                while (!done) ;
                disposable.Dispose();

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item3", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.DELETE, updateItem));
                while (!done) ;
                disposable.Dispose();

                Assert.Single(fixture.CRUDListState.Value);
                Assert.Equal(2, stateChangeCount);

                disposable = subscribeTest();
                fixture.CRUDListCmds.Transform((ServiceFixture.IntListVP.CMD.CLEAR, null));
                while (!done) ;
                disposable.Dispose();

                Assert.Empty(fixture.CRUDListState.Value);
                Assert.Equal(2, stateChangeCount);
            }
        }
    }
}