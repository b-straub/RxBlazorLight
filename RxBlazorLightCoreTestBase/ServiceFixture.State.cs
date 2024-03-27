
namespace RxBlazorLightCoreTestBase
{
    public partial class ServiceFixture
    {
        public Func<bool> CanChangeNotZero => () => IntStateResult > 0;
        public Func<bool> CanChangeBelow(int upperBound) => () => IntStateResult < upperBound;

        public Action Reset => () => IntStateResult = 0;
        public Func<Task> ResetAsync => () => { IntStateResult = 0; return Task.CompletedTask; };

        public Action Increment => () => IntStateResult++;

        public Action Add(int value)
        {
            return () => IntStateResult += value;
        }

        public Func<Task> AddAsync(int value)
        {
            return async () =>
            {
                if (IntStateResult > 0)
                {
                    throw new InvalidOperationException("AddAsync");
                }

                await Task.Delay(1000);
                IntStateResult += value;
            };
        }

        public Func<CancellationToken, Task> MultiplyAsync(int value)
        {
            return async ct =>
            {
                await Task.Delay(1000, ct);
                IntStateResult *= value;
            };
        }

        public enum CMD_CRUD
        {
            ADD,
            UPDATE,
            DELETE,
            CLEAR
        }

        public Func<CancellationToken, Task> ChangeCrudListAsync((CMD_CRUD CMD, CRUDTest? ITEM) value)
        {
            return async ct =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    var item = CRUDList.Where(i => i.Id == value.ITEM.Id).FirstOrDefault();
                    ArgumentNullException.ThrowIfNull(item);

                    CRUDList.Remove(item);
                    CRUDList.Add(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    CRUDList.Remove(value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
                {
                    CRUDList.Clear();
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                await Task.Delay(5, ct);
            };
        }

        public Func<CancellationToken, Task> ChangeCrudDictAsync((CMD_CRUD CMD, Guid? ID, CRUDTest? ITEM) value)
        {
            return async ct =>
            {
                if (value.CMD is CMD_CRUD.ADD)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Add(value.ID.Value, value.ITEM);
                }
                else if (value.CMD is CMD_CRUD.UPDATE)
                {
                    ArgumentNullException.ThrowIfNull(value.ITEM);
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict[value.ID.Value] = value.ITEM;
                }
                else if (value.CMD is CMD_CRUD.DELETE)
                {
                    ArgumentNullException.ThrowIfNull(value.ID);
                    CRUDDict.Remove(value.ID.Value);
                }
                else if (value.CMD is CMD_CRUD.CLEAR)
                {
                    CRUDDict.Clear();
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
