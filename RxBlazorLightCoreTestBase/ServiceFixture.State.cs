﻿
using RxBlazorLightCore;
using static RxBlazorLightCoreTestBase.ServiceFixture.IntListVP;

namespace RxBlazorLightCoreTestBase
{
    public partial class ServiceFixture
    {
        public class IncremementVP(ServiceFixture service, IState<int> state) : AsyncValueProvider<ServiceFixture, int>(service, state)
        {
            protected override Task<int> ProvideValueAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(State.Value + 1);
            }
        }

        public class AddVP(ServiceFixture service, IState<int> state) : AsyncValueProvider<ServiceFixture, int, int>(service, state)
        {
            protected override Task<int> ProvideValueAsync(int value, CancellationToken cancellationToken)
            {
                if (State.Value > 0)
                {
                    throw new InvalidOperationException("AddVP");
                }
                return Task.FromResult(State.Value + value);
            }
        }

        public class AsyncIntX(ServiceFixture service, IState<int> state) : AsyncValueProvider<ServiceFixture, int, int>(service, state)
        {
            protected override async Task<int> ProvideValueAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(1000, cancellationToken);
                return State.Value * value;
            }
        }

        public class ChangeTestSP(ServiceFixture service, IState state) : AsyncStateProvider<ServiceFixture, string>(service, state)
        {
            protected override async Task ProvideStateAsync(string? valueIn, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(valueIn);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                Service.Test = valueIn;
            }

            public override bool CanCancel()
            {
                return true;
            }
        }

        public class ChangeTestSyncSP(ServiceFixture service, IState state) : StateProvider<ServiceFixture>(service, state)
        {
            protected override void ProvideState()
            {
                Service.Test = "Sync";
            }
        }

        public class IntListVP(ServiceFixture service, IState<IEnumerable<CRUDTest>, IList<CRUDTest>> state) :
            AsyncValueRefProvider<ServiceFixture, (CMD CMD, CRUDTest? ITEM), IEnumerable<CRUDTest>, IList<CRUDTest>>(service, state)
        {
            public enum CMD
            {
                ADD,
                UPDATE,
                DELETE,
                CLEAR
            }

            protected override Task ProvideValueAsync((CMD CMD, CRUDTest? ITEM) value, IList<CRUDTest> stateRef, CancellationToken cancellationToken)
            {
                if (value.CMD is CMD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    stateRef.Add(value.ITEM);
                }
                else if (value.CMD is CMD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = stateRef.Where(i => i.Id == value.ITEM.Id).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(item);

                    stateRef.Remove(item);
                    stateRef.Add(value.ITEM);
                }
                else if (value.CMD is CMD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    stateRef.Remove(value.ITEM);
                }
                else if (value.CMD is CMD.CLEAR)
                {
                    stateRef.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                return Task.CompletedTask;
            }
        }
    }
}
