
using RxBlazorLightCore;

namespace RxBlazorLightCoreTestBase
{
    public partial class ServiceFixture
    {
        public class IncremementVP(ServiceFixture service, IState<int> state) : StateProviderAsync<ServiceFixture, int>(service, state)
        {
            protected override Task<int> ProvideValueAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(State.Value + 1);
            }
        }

        public class AddVP(ServiceFixture service, IState<int> state) : StateTransformerAsync<ServiceFixture, int, int>(service, state)
        {
            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                if (State.Value > 0)
                {
                    throw new InvalidOperationException("AddVP");
                }

                await Task.Delay(5, cancellationToken);
                return State.Value + value;
            }
        }

        public class AsyncIntX(ServiceFixture service, IState<int> state) : StateTransformerAsync<ServiceFixture, int, int>(service, state)
        {
            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(1000, cancellationToken);
                return State.Value * value;
            }
        }

        public class ChangeTestSP(ServiceFixture service) : ServiceStateTransformerAsync<ServiceFixture, string>(service)
        {
            public override bool CanCancel => true;

            protected override async Task TransformStateAsync(string? valueIn, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(valueIn);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                Service.Test = valueIn;
            }
        }

        public class ChangeTestSyncSP(ServiceFixture service) : ServiceStateProvider<ServiceFixture>(service)
        {
            protected override void ProvideState()
            {
                Service.Test = "Sync";
            }
        }

        public class IntListVP(ServiceFixture service, IState<IEnumerable<CRUDTest>, List<CRUDTest>> state) :
            StateRefTransformerAsync<ServiceFixture, (IntListVP.CMD_LIST CMD, CRUDTest? ITEM), IEnumerable<CRUDTest>, List<CRUDTest>>(service, state)
        {
            public enum CMD_LIST
            {
                ADD,
                UPDATE,
                DELETE,
                CLEAR
            }

            protected override async Task TransformStateAsync((CMD_LIST CMD, CRUDTest? ITEM) value, List<CRUDTest> stateRef, CancellationToken cancellationToken)
            {
                if (value.CMD is CMD_LIST.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    stateRef.Add(value.ITEM);
                }
                else if (value.CMD is CMD_LIST.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = stateRef.Where(i => i.Id == value.ITEM.Id).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(item);

                    stateRef.Remove(item);
                    stateRef.Add(value.ITEM);
                }
                else if (value.CMD is CMD_LIST.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    stateRef.Remove(value.ITEM);
                }
                else if (value.CMD is CMD_LIST.CLEAR)
                {
                    stateRef.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, cancellationToken);
            }
        }

        public class IntDictVP(ServiceFixture service, IState<IEnumerable<KeyValuePair<Guid, CRUDTest>>, Dictionary<Guid, CRUDTest>> state) :
            StateRefTransformerAsync<ServiceFixture, (IntDictVP.CMD_DICT CMD, Guid? ID, CRUDTest? ITEM), IEnumerable<KeyValuePair<Guid, CRUDTest>>, Dictionary<Guid, CRUDTest>>(service, state)
        {
            public enum CMD_DICT
            {
                ADD,
                UPDATE,
                DELETE,
                CLEAR
            }

            protected override async Task TransformStateAsync((CMD_DICT CMD, Guid? ID, CRUDTest? ITEM) value, Dictionary<Guid, CRUDTest> stateRef, CancellationToken cancellationToken)
            {
                if (value.CMD is CMD_DICT.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    stateRef.Add(value.ID.Value, value.ITEM);
                }
                else if (value.CMD is CMD_DICT.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    stateRef[value.ID.Value] = value.ITEM;
                }
                else if (value.CMD is CMD_DICT.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ID);
                    stateRef.Remove(value.ID.Value);
                }
                else if (value.CMD is CMD_DICT.CLEAR)
                {
                    stateRef.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, cancellationToken);
            }
        }
    }
}
