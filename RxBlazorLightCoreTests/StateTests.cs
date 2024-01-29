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

                if (fixture.Increment.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Increment.Phase}, ID {sc.ID}, VPID {fixture.Increment.ID}");
            }, 0);

            fixture.IntState.Run(0);

            Assert.Equal(0, fixture.IntState.Value);

            fixture.Increment.Run();
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

                if (fixture.Add.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Add.Phase}, ID {sc.ID}, VPID {fixture.Add.ID}");
            }, 0);

            fixture.IntState.Run(0);

            Assert.Equal(0, fixture.IntState.Value);

            fixture.Add.Run(10);
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

                if (fixture.IntStateAsyncX.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntStateAsyncX.Phase}, ID {sc.ID}, VPID {fixture.IntStateAsyncX.ID}");
            }, 0);

            Assert.Equal(10, fixture.IntStateAsyncX.Value);
            fixture.IntStateAsyncX.Run(5);
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

                if (fixture.Add.Phase is ValueProviderPhase.EXCEPTION &&
                    fixture.Exceptions.First().Exception.Message == "AddVP")
                {
                    exception = true;
                }

                if (fixture.Add.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.Add.Phase}, ID {sc.ID}, VPID {fixture.Add.ID}");
            }, 0);

            fixture.ResetExceptions();
            fixture.IntState.Run(0);

            Assert.Equal(0, fixture.IntState.Value);
            fixture.Add.Run(10);
            while (!done) ;
            Assert.Equal(10, fixture.IntState.Value);
            Assert.Equal(2, stateChangeCount);

            done = false;
            stateChangeCount = 0;

            fixture.Add.Run(1);
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

                if (fixture.ChangeTest.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTest.Phase}, ID {sc.ID}, VPID {fixture.ChangeTest.ID}");
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTest.Run("Test");
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

                if (fixture.ChangeTestSync.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTestSync.Phase}, ID {sc.ID}, VPID {fixture.ChangeTestSync.ID}");
            }, 0);

            fixture.ClearTest();

            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTestSync.Run();
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

                    if (fixture.ChangeTest.Phase is ValueProviderPhase.CANCELED)
                    {
                        canceled = true;
                    }

                    if (fixture.ChangeTest.Done())
                    {
                        done = true;
                    }

                    _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.ChangeTest.Phase}, ID {sc.ID}, VPID {fixture.ChangeTest.ID}");
                }
            }, 0);

            fixture.ClearTest();
            Assert.Equal(string.Empty, fixture.Test);

            fixture.ChangeTest.Run("Test");
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

            fixture.Subscribe(sc =>
            {
                if (!done && sc.ID == fixture.CRUDListCmds.ID)
                {
                    stateChangeCount++;
                }

                if (fixture.CRUDListCmds.Done())
                {
                    done = true;
                }

                _output.WriteLine($"Done {done}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDListCmds.Phase}, ID {sc.ID}, VPID {fixture.CRUDListCmds.ID}");

            }, 0);

            fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.CLEAR, null));

            Assert.True(fixture.CRUDListState.HasValue());

            if (fixture.CRUDListState.HasValue())
            {
                Assert.Empty(fixture.CRUDListState.Value);

                fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.ADD, new CRUDTest("Item1", Guid.NewGuid())));
                while (!done) ;

                Assert.Single(fixture.CRUDListState.Value);
                Assert.Equal("Item1", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                done = false;
                stateChangeCount = 0;
                fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.ADD, new CRUDTest("Item2", Guid.NewGuid())));
                while (!done) ;

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item2", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                var lastItem = fixture.CRUDListState.Value.Last();
                var updateItem = lastItem with { Item = "Item3" };

                done = false;
                stateChangeCount = 0;
                fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.UPDATE, updateItem));
                while (!done) ;

                Assert.Equal(2, fixture.CRUDListState.Value.Count());
                Assert.Equal("Item3", fixture.CRUDListState.Value.Last().Item);
                Assert.Equal(2, stateChangeCount);

                done = false;
                stateChangeCount = 0;
                fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.DELETE, updateItem));
                while (!done) ;

                Assert.Single(fixture.CRUDListState.Value);
                Assert.Equal(2, stateChangeCount);

                done = false;
                stateChangeCount = 0;
                fixture.CRUDListCmds.Run((ServiceFixture.IntListVP.CMD.CLEAR, null));
                while (!done) ;

                Assert.Empty(fixture.CRUDListState.Value);
                Assert.Equal(2, stateChangeCount);
            }
        }
    }
}