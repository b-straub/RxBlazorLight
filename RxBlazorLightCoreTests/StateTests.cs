using Microsoft.Extensions.Time.Testing;
using RxBlazorLightCore;
using RxBlazorLightCoreTestBase;
using Xunit.Abstractions;
using R3;

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

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.IntState.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.IntState.StateID}");

                if (sc.StateID == fixture.IntState.StateID)
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
        public void TestIntStateCommand()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.IntState.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.IntState.StateID}");

                if (sc.StateID == fixture.IntState.StateID)
                {
                    stateChangeCount++;
                }
            });

            fixture.IntState.Value = 0;
            Assert.Equal(0, fixture.IntState.Value);

            fixture.IncrementState(fixture.IntState);
            Assert.Equal(1, fixture.IntState.Value);

            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestIntStateCommandAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.IntState.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.IntState.StateID}");

                if (sc.StateID == fixture.IntState.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            fixture.IntState.Value = 0;
            Assert.Equal(0, fixture.IntState.Value);

            await fixture.IncrementStateAsync(fixture.IntState);
            while (!done) ;

            Assert.Equal(1, fixture.IntState.Value);

            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestIntStateCommandAsyncCancel()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.IntState.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.IntState.StateID}");

                if (sc.StateID == fixture.IntState.StateID || sc.StateID == fixture.IntCommandAsync.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.IntCommandAsync.Changing())
                {
                    fixture.IntCommandAsync.Cancel();
                }

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            fixture.IntState.Value = 0;
            Assert.Equal(0, fixture.IntState.Value);

            await fixture.IncrementStateAsyncC(fixture.IntState);
            while (!done) ;

            Assert.Equal(0, fixture.IntState.Value);

            Assert.Equal(3, stateChangeCount);
        }

        [Fact]
        public void TestIncrement()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommand.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommand.Phase}, ID {sc.StateID}, VPID {fixture.IntCommand.StateID}");

                if (fixture.IntCommand.Done())
                {
                    done = true;
                }
            });

            fixture.IntCommand.Execute(fixture.Reset);
            while (!done) ;
            Assert.False(fixture.CanChangeNotZero());
            Assert.Equal(0, fixture.IntStateResult);

            fixture.IntCommand.Execute(fixture.Increment);
            while (!done) ;
            Assert.True(fixture.CanChangeNotZero());

            Assert.Equal(1, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestCommandNotNotify()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (sc.StateID == fixture.IntCommandAsync.StateID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                    if (fixture.IntCommandAsync.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(fixture.ResetAsync, true);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);
            disposable.Dispose();

            disposable = SubscribeTest();
            // ReSharper disable once NotResolvedInText
            await fixture.IntCommandAsync.ExecuteAsync(_ => throw new ArgumentOutOfRangeException("Test"), true);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);
            Assert.Equal(1, stateChangeCount);
            disposable.Dispose();

            disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(async _ => { await Task.Delay(10); fixture.IntCommandAsync.NotifyChanging(); fixture.IntStateResult = 1; }, true);
            while (!done) ;
            Assert.Equal(1, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            disposable = SubscribeTest();
            // ReSharper disable once NotResolvedInText
            await fixture.IntCommandAsync.ExecuteAsync(async _ => { await Task.Delay(10); throw new ArgumentOutOfRangeException("Test"); }, true);
            while (!done) ;
            Assert.Equal(1, fixture.IntStateResult);
            Assert.Equal(1, stateChangeCount);
            disposable.Dispose();
        }

        [Fact]
        public async Task TestStateNotNotify()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (sc.StateID == fixture.IntCommandAsync.StateID || sc.StateID == fixture.IntState.StateID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                    if (fixture.IntCommandAsync.Done())
                    {
                        done = true;
                    }
                });
            }

            fixture.IntState.Value = 0;
            Assert.Equal(0, fixture.IntState.Value);

            var disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(async _ => { await Task.Delay(10); fixture.IntState.SetValueSilent(1); });
            while (!done) ;
            Assert.Equal(1, fixture.IntState.Value);
            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(async _ => { await Task.Delay(10); fixture.IntState.Value = 2; });
            while (!done) ;
            Assert.Equal(2, fixture.IntState.Value);
            Assert.Equal(3, stateChangeCount);
            disposable.Dispose();
        }

        [Fact]
        public void TestAdd()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommand.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommand.Phase}, ID {sc.StateID}, VPID {fixture.IntCommand.StateID}");

                if (fixture.IntCommand.Done())
                {
                    done = true;
                }
            });

            fixture.IntCommand.Execute(fixture.Reset);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);

            fixture.IntCommand.Execute(fixture.Add(10));
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

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.IntCommandAsync.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                if (fixture.IntCommandAsync.Done())
                {
                    done = true;
                }
            });

            fixture.IntStateResult = 10;

            Assert.Equal(10, fixture.IntStateResult);
            Assert.True(fixture.CanChangeBelow(20)());

            await fixture.IntCommandAsync.ExecuteAsync(fixture.MultiplyAsync(5));
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

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.IntCommandAsync.StateID)
                    {
                        stateChangeCount++;
                    }

                    if (fixture.IntCommandAsync.Phase is StatePhase.EXCEPTION &&
                        fixture.Exceptions.First().Exception.Message == "AddAsync")
                    {
                        exception = true;
                    }

                    _output.WriteLine($"Done {fixture.IntCommandAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.IntCommandAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                    if (fixture.IntCommandAsync.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = SubscribeTest();
            fixture.ResetExceptions();
            await fixture.IntCommandAsync.ExecuteAsync(fixture.ResetAsync);
            while (!done) ;
            Assert.Equal(0, fixture.IntStateResult);
            disposable.Dispose();

            disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(fixture.AddAsync(10));
            while (!done) ;
            Assert.Equal(10, fixture.IntStateResult);
            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            disposable = SubscribeTest();
            await fixture.IntCommandAsync.ExecuteAsync(fixture.AddAsync(1));
            while (!done) ;
            Assert.True(exception);
            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();
        }

        [Fact]
        public void TestStateObservable()
        {
            ServiceFixture fixture = new();
            var phaseChangeCount = 0;
            bool completed = false;
            bool initialized = false;

            fixture.AsObservable.Subscribe(p =>
            {
                phaseChangeCount++;

                _output.WriteLine($"Value {fixture.IntStateResult}, PCC {phaseChangeCount} Phase {p}");

                if (fixture.IntCommand.Phase is StatePhase.CHANGED)
                {
                    initialized = fixture.IntStateResult == 0;
                    completed = !completed && fixture.IntStateResult == 2;
                }
            });

            fixture.IntCommand.Execute(fixture.Reset);
            while (!initialized) ;
            Assert.Equal(0, fixture.IntStateResult);

            var changer = Observable.Range(0, 3);

            var fakeTimeProvider = new FakeTimeProvider();
            
            var disposable = changer
                .ObserveOn(fakeTimeProvider)
                .SubscribeOn(fakeTimeProvider)
                .Subscribe(r => fixture.IntCommand.Execute(() => { fixture.IntStateResult = r; }));

            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            
            while (!completed) ;
            Assert.Equal(4, phaseChangeCount);
            disposable.Dispose();
        }

        [Fact]
        public async Task TestAsObservable()
        {
            ServiceFixture fixture = new();
            var asObservableCount = 0;
            var changedValue = 0;
          
            var disposableO = fixture.AsObservable(fixture.IntCommandAsync)
                .Subscribe(_ => asObservableCount++);

            var disposableOc = fixture.AsChangedObservable(fixture.IntState)
                .Subscribe(v => changedValue += v);

            await fixture.IntCommandAsync.ExecuteAsync(fixture.AddAsync(1));
            await fixture.IntCommandAsync.ExecuteAsync(fixture.AddAsync(1));

            fixture.IntState.Value = 1;
            fixture.IntState.Value = 1;

            disposableO.Dispose();
            disposableOc.Dispose();

            Assert.Equal(4, asObservableCount);
            Assert.Equal(2, changedValue);
        }

        [Fact]
        public async Task TestCrudList()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            var done = false;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CRUDListCommand.StateID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDListCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDListCommand.Phase}, ID {sc.StateID}, VPID {fixture.CRUDListCommand.StateID}");

                    if (fixture.CRUDListCommand.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDList);
            disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.ADD, new CRUDTest("Item1", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDList);
            Assert.Equal("Item1", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.ADD, new CRUDTest("Item2", Guid.NewGuid()))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDList.Count);
            Assert.Equal("Item2", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDList.Last();
            var updateItem = lastItem with { Item = "Item3" };

            disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.UPDATE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDList.Count);
            Assert.Equal("Item3", fixture.CRUDList.Last().Item);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.DELETE, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDList);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            await fixture.CRUDListCommand.ExecuteAsync(fixture.ChangeCrudListAsync((ServiceFixture.CmdCRUD.CLEAR, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Empty(fixture.CRUDList);
            Assert.Equal(2, stateChangeCount);
        }

        [Fact]
        public async Task TestCrudDictCommand()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            var done = false;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CRUDDictCommand.StateID)
                    {
                        stateChangeCount++;
                    }

                    _output.WriteLine($"Done {fixture.CRUDDictCommand.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CRUDDictCommand.Phase}, ID {sc.StateID}, VPID {fixture.CRUDDictCommand.StateID}");

                    if (fixture.CRUDDictCommand.Done())
                    {
                        done = true;
                    }
                });
            }

            var disposable = SubscribeTest();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.CLEAR, default, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, stateChangeCount);
            disposable.Dispose();

            Assert.Empty(fixture.CRUDDict);
            disposable = SubscribeTest();
            var addGuid1 = Guid.NewGuid();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.ADD, addGuid1, new CRUDTest("Item1", addGuid1))));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDict);
            Assert.Equal("Item1", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            var addGuid2 = Guid.NewGuid();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.ADD, addGuid2, new CRUDTest("Item2", addGuid2))));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDict.Count);
            Assert.Equal("Item2", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            var lastItem = fixture.CRUDDict.Last().Value;
            var updateItem = lastItem with { Item = "Item3" };

            disposable = SubscribeTest();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.UPDATE, updateItem.Id, updateItem)));
            while (!done) ;
            disposable.Dispose();

            Assert.Equal(2, fixture.CRUDDict.Count);
            Assert.Equal("Item3", fixture.CRUDDict.Last().Value.Item);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.DELETE, updateItem.Id, null)));
            while (!done) ;
            disposable.Dispose();

            Assert.Single(fixture.CRUDDict);
            Assert.Equal(2, stateChangeCount);

            disposable = SubscribeTest();
            await fixture.CRUDDictCommand.ExecuteAsync(fixture.ChangeCrudDictAsync((ServiceFixture.CmdCRUD.CLEAR, default, null)));
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

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroup.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroup.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroup.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroup.StateID}");

                if (fixture.EnumStateGroup.Done())
                {
                    done = true;
                }
            });

            fixture.EnumStateGroup.ChangeValue(TestEnum.THREE);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);
            Assert.Equal(1, stateChangeCount);
        }

        [Fact]
        public void TestStateGroupSync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroup.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroup.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroup.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroup.StateID}");

                if (fixture.EnumStateGroup.Done())
                {
                    done = true;
                }
            });

            fixture.EnumStateGroup.ChangeValue(TestEnum.THREE, fixture.ValueChanging);
            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroup.Value);
            Assert.Equal(TestEnum.ONE, fixture.EnumStateGroupOldValue);
            Assert.Equal(1, stateChangeCount);
        }

        [Fact]
        public async Task TestStateGroupAsync()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                if (sc.StateID == fixture.EnumStateGroupAsync.StateID)
                {
                    stateChangeCount++;
                }

                _output.WriteLine($"Done {fixture.EnumStateGroupAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.EnumStateGroupAsync.Phase}, ID {sc.StateID}, VPID {fixture.EnumStateGroupAsync.StateID}");

                if (fixture.EnumStateGroupAsync.Done())
                {
                    done = true;
                }
            });

            await fixture.EnumStateGroupAsync.ChangeValueAsync(TestEnum.THREE, fixture.ValueChangingAsync);

            while (!done) ;
            Assert.Equal(TestEnum.THREE, fixture.EnumStateGroupAsync.Value);
            Assert.Equal(TestEnum.ONE, fixture.EnumStateGroupAsyncOldValue);
            Assert.Equal(2, stateChangeCount);
        }
        
        [Fact]
        public async Task TestStateObserver()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.CancellableObserverAsync.StateID}");

                if (sc.StateID == fixture.CancellableObserverAsync.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.CancellableObserverAsync.Done())
                {
                    done = true;
                }
            });

            fixture.CancellableObserverAsync.ExecuteAsync(fixture.ObserveState);
            while (!done)
            {
                await Task.Delay(5);
            }
            
            Assert.Equal(3, stateChangeCount);
        }
        
        [Fact]
        public async Task TestStateObserverCancel()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.CancellableObserverAsync.StateID}");

                if (sc.StateID == fixture.CancellableObserverAsync.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.CancellableObserverAsync.Changing())
                {
                    fixture.CancellableObserverAsync.Cancel();
                }

                if (fixture.CancellableObserverAsync.Done())
                {
                    done = true;
                }
            });
            
            fixture.CancellableObserverAsync.ExecuteAsync(fixture.ObserveState);
            while (!done)
            {
                await Task.Delay(5);
            }
            Assert.Equal(2, stateChangeCount);
        }
        
        [Fact]
        public async Task TestStateObserverComplex()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.CancellableObserverAsync.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.CancellableObserverAsync.StateID}");

                if (sc.StateID == fixture.CancellableObserverAsync.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.CancellableObserverAsync.Done())
                {
                    done = true;
                }
            });

            fixture.CancellableObserverAsync.ExecuteAsync(obs => fixture.ObserveStateComplex(obs, _output));
            while (!done)
            {
                await Task.Delay(5);
            }
            
            Assert.InRange(stateChangeCount, 3, 5);
            Assert.Equal(20, fixture.IntState.Value);
        }
        
        [Fact]
        public async Task TestStateObserverComplexCancel()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool done = false;

            fixture.AsObservable.Subscribe(sc =>
            {
                _output.WriteLine($"Value {fixture.CancellableObserverAsync.Value}, CC {stateChangeCount} Reason {sc.Reason}, ID {sc.StateID}, VPID {fixture.CancellableObserverAsync.StateID}");

                if (sc.StateID == fixture.CancellableObserverAsync.StateID)
                {
                    stateChangeCount++;
                }

                if (fixture.CancellableObserverAsync.Changing() && stateChangeCount == 2)
                {
                    fixture.CancellableObserverAsync.Cancel();
                }

                if (fixture.CancellableObserverAsync.Done())
                {
                    done = true;
                }
            });
            
            fixture.CancellableObserverAsync.ExecuteAsync(obs => fixture.ObserveStateComplex(obs, _output));
            while (!done)
            {
                await Task.Delay(5);
            }
            Assert.Equal(3, stateChangeCount);
            Assert.Equal(10, fixture.IntState.Value);
        }
        
        [Fact]
        public async Task TestStateObserverThrow()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool exception = false;
            bool done = false;

            var disposable = SubscribeTest();
            fixture.ResetExceptions();
            fixture.CancellableObserverAsync.ExecuteAsync(ServiceFixture.ObserveStateThrow);
            while (!done)
            {
                await Task.Delay(5);
            }
            Assert.True(exception);
            Assert.Equal(2, stateChangeCount);
            Assert.Single(fixture.Exceptions);
            disposable.Dispose();
            return;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CancellableObserverAsync.StateID)
                    {
                        stateChangeCount++;
                    }

                    if (fixture.CancellableObserverAsync.Phase is StatePhase.EXCEPTION &&
                        fixture.Exceptions.First().Exception.GetType() == typeof(TimeoutException))
                    {
                        exception = true;
                    }

                    _output.WriteLine($"Done {fixture.CancellableObserverAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CancellableObserverAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                    if (fixture.CancellableObserverAsync.Done())
                    {
                        done = true;
                    }
                });
            }
        }
        
        [Fact]
        public async Task TestStateObserverHandleErrorThrow()
        {
            ServiceFixture fixture = new();
            var stateChangeCount = 0;
            bool exception = false;
            bool done = false;

            var disposable = SubscribeTest();
            fixture.ResetExceptions();
            fixture.CancellableObserverHandleErrorAsync.ExecuteAsync(ServiceFixture.ObserveStateThrow);
            while (!done)
            {
                await Task.Delay(5);
            }
            Assert.True(exception);
            Assert.Equal(2, stateChangeCount);
            Assert.Empty(fixture.Exceptions);
            disposable.Dispose();
            return;

            IDisposable SubscribeTest()
            {
                done = false;
                stateChangeCount = 0;

                return fixture.AsObservable.Subscribe(sc =>
                {
                    if (!done && sc.StateID == fixture.CancellableObserverHandleErrorAsync.StateID)
                    {
                        stateChangeCount++;
                    }

                    if (fixture.CancellableObserverHandleErrorAsync.Phase is StatePhase.EXCEPTION &&
                        fixture.CancellableObserverHandleErrorAsync.Exception?.GetType() == typeof(TimeoutException))
                    {
                        exception = true;
                    }

                    _output.WriteLine($"Done {fixture.CancellableObserverHandleErrorAsync.Done()}, CC {stateChangeCount} Reason {sc.Reason}, Phase {fixture.CancellableObserverHandleErrorAsync.Phase}, ID {sc.StateID}, VPID {fixture.IntCommandAsync.StateID}");

                    if (fixture.CancellableObserverHandleErrorAsync.Done())
                    {
                        done = true;
                    }
                });
            }
        }
    }
}