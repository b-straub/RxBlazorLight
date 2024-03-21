using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{

    public record CRUDToDoItem
    (
        string Text,
        DateTime DueDate,
        bool Completed,
        Guid? Id = null
    );

    public enum DBRole
    {
        Admin,
        User,
        Guest
    }

    public class CrudService : RxBLService
    {
        public sealed partial class CrudItemScope(CrudService service, CRUDToDoItem? item) : RxBLScope<CrudService>(service)
        {
            public IState<string> Text { get; } = service.CreateState(item is null ? string.Empty : item.Text);
            public IState<DateTime> DueDateDate { get; } = service.CreateState(item is null ? DateTime.Now.Date : item.DueDate.Date);
            public IState<TimeSpan> DueDateTime { get; } = service.CreateState(item is null ? DateTime.Now.TimeOfDay : item.DueDate.TimeOfDay);

            private readonly Guid _id = item?.Id ?? Guid.NewGuid();

            public async Task SubmitAsync()
            {
                var newItem = new CRUDToDoItem(Text.Value, DueDateDate.Value + DueDateTime.Value, false, _id);
                await Service.CRUDItemDB.ChangeAsync(AddCRUDItem(newItem));
            }
            public bool CanSubmit()
            {
                return Text.Value != string.Empty && DueDateDate.Value.Date >= DateTime.Now.Date;
            }

            public Func<IState<string>, bool> CanUpdateText => _ =>
            {
                return Service.CanUpdateText;
            };

            public Func<IState<DateTime>, bool> CanUpdateDueDate => s =>
            {
                return Service.CanUpdateDueDate;
            };

            public Func<IState<TimeSpan>, bool> CanUpdateTime => s =>
            {
                return Service.CanUpdateDueDate;
            };
        }

        public IStateAsync<IDictionary<Guid, CRUDToDoItem>> CRUDItemDB { get; }

        public IStateGroup<DBRole> CRUDDBRoleGroup { get; }

        public CrudService()
        {
            CRUDItemDB = this.CreateStateAsync<IDictionary<Guid, CRUDToDoItem>>(new Dictionary<Guid, CRUDToDoItem>());
            CRUDDBRoleGroup = this.CreateStateGroup([DBRole.Admin, DBRole.User, DBRole.Guest], DBRole.Admin);
        }

        public CrudItemScope CreateItemScope(CRUDToDoItem? item = null)
        {
            return new CrudItemScope(this, item);
        }

        public Func<IState<DBRole>, bool> CanChangeRole => _ => !CRUDItemDB.Changing();

        public bool CanAdd => CRUDDBRoleGroup.Value is DBRole.Admin && !CRUDItemDB.Changing();
        public bool CanUpdate => (CanUpdateText || CanUpdateDueDate) && !CRUDItemDB.Changing();

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> AddCRUDItem(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                s.Value[item.Id!.Value] = item;
                await Task.Delay(100);
            };
        }

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> UpdateCRUDItemAsync(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                s.Value[item.Id!.Value] = item;
                await Task.Delay(1000);
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanToggleCRUDItemCompleted => _ => CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User;

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> ToggleCRUDItemCompletedAsync(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                item = item with { Completed = !item.Completed };
                s.Value[item.Id!.Value] = item;
                await Task.Delay(2000);
            };
        }

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> RemoveCRUDItem(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                s.Value.Remove(item.Id!.Value);
                await Task.Delay(100);
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanRemoveCompletedCRUDItems => s =>
        {
            return s.Value.Values.Where(x => x.Completed).Any() && (CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User);
        };


        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> RemoveCompletedCRUDItems()
        {
            return async s =>
            {
                foreach (var item in s.Value.Values)
                {
                    if (item.Completed)
                    {
                        s.Value.Remove(item.Id!.Value);
                    }
                }
                await Task.Delay(100);
            };
        }

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanRemoveAllCRUDItems => s =>
        {
            return s.Value.Values.Count != 0;
        };

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> RemoveAllCRUDItems()
        {
            return async s =>
            {
                s.Value.Clear();
                await Task.Delay(100);
            };
        }

        private bool CanUpdateText => CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User;
        private bool CanUpdateDueDate => CRUDDBRoleGroup.Value is DBRole.Admin;
    }
}
