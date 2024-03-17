
using RxBlazorLightCore;

namespace RxBlazorLightCoreTestBase
{
    public partial class ServiceFixture
    {
        public static Func<IStateBase<int>, bool> CanChangeNotZero => s => s.Value > 0;
        public static Func<IStateBase<int>, bool> CanChangeBelow(int upperBound) => s => s.Value < upperBound;

        public static Action<IState<int>> Increment => s => s.Value++;

        public static Action<IState<int>> Add(int value)
        {
            return s => s.Value += value;
        }

        public static Func<IStateAsync<int>, Task> AddAsync(int value)
        {
            return async s =>
            {
                if (s.Value > 0)
                {
                    throw new InvalidOperationException("AddAsync");
                }

                await Task.Delay(1000);
                s.Value += value;
            };
        }

        public static Func<IStateAsync<int>, CancellationToken, Task> MultiplyAsync(int value)
        {
            return async (s, ct) =>
            {
                await Task.Delay(1000, ct);
                s.Value *= value;
            };
        }

        public static readonly Action<IRxBLService> SetTestStringDirect = s => ((ServiceFixture)s).Test = "Sync";

        public static readonly Func<IRxBLService, Task> SetTestStringAsyncDirect = async s =>
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            ((ServiceFixture)s).Test = "Async";
        };

        public static Action<IRxBLService> SetTestString(string value)
        {
            return s => ((ServiceFixture)s).Test = value;
        }

        public static Func<IRxBLService, Task> SetTestStringAsync(string value)
        {
            return async s =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                ((ServiceFixture)s).Test = value;
            };
        }

        public static Func<IRxBLService, CancellationToken, Task> SetTestStringAsyncLR(string value)
        {
            return async (s, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
                ((ServiceFixture)s).Test = value;
            };
        }

        public enum CMD_CRUD
        {
            ADD,
            UPDATE,
            DELETE,
            CLEAR
        }

        public static Func<IStateAsync<IList<CRUDTest>>, CancellationToken, Task> ChangeCrudListAsync((CMD_CRUD CMD, CRUDTest? ITEM) value)
        {
            return async (s, ct) =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    s.Value.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = s.Value.Where(i => i.Id == value.ITEM.Id).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(item);

                    s.Value.Remove(item);
                    s.Value.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    s.Value.Remove(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
                {
                    s.Value.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, ct);
            };
        }

        public static Func<IStateAsync<IDictionary<Guid, CRUDTest>>, CancellationToken, Task> ChangeCrudDictAsync((CMD_CRUD CMD, Guid? ID, CRUDTest? ITEM) value)
        {
            return async (s, ct) =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    s.Value.Add(value.ID.Value, value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    s.Value[value.ID.Value] = value.ITEM;
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ID);
                    s.Value.Remove(value.ID.Value);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
                {
                    s.Value.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, ct);
            };
        }
    }
}
