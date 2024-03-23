using MudBlazor;
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
        public sealed partial class CrudItemInput(CrudService service, CRUDToDoItem? item)
        {
            public IState<string> Text { get; } = service.CreateState(item is null ? string.Empty : item.Text);
            public IState<DateTime> DueDateDate { get; } = service.CreateState(item is null ? DateTime.Now.Date : item.DueDate.Date);
            public IState<TimeSpan> DueDateTime { get; } = service.CreateState(item is null ? DateTime.Now.TimeOfDay : item.DueDate.TimeOfDay);

            private readonly CRUDToDoItem? _item = item;

            public async Task SubmitAsync()
            {
                var newItem = new CRUDToDoItem(Text.Value, DueDateDate.Value + DueDateTime.Value, false, _item?.Id ?? Guid.NewGuid());
                await service.CRUDItemDB.ChangeAsync(AddCRUDItem(newItem));
            }
            public bool CanSubmit()
            {
                var dateItem = NoSeconds(_item?.DueDate);
                var dateNew = NoSeconds(DueDateDate.Value.Date + DueDateTime.Value);

                return Text.Value != string.Empty && DueDateDate.Value.Date >= DateTime.Now.Date &&
                    (Text.Value != _item?.Text || dateNew != dateItem);
            }

            public Func<IState<string>, bool> CanUpdateText => _ =>
            {
                return service.CanUpdateText;
            };

            public Func<IState<DateTime>, bool> CanUpdateDueDate => s =>
            {
                return service.CanUpdateDueDate;
            };

            public static Func<IState<string>, StateValidation> ValidateText => s =>
            {
                return new("Text can not be empty!", s.Value.Length == 0);
            };

            public static Func<IState<DateTime>, StateValidation> ValidateDueDate => s =>
            {
                return new("DueDate can not be in the past!", s.Value.Date < DateTime.Now.Date);
            };

            public Func<IState<TimeSpan>, StateValidation> ValidateDueDateTime => s =>
            {
                var dateNowNS = NoSeconds(DateTime.Now);
                var dateNew = _item is null ? NoSeconds(DueDateDate.Value.Date + s.Value) : dateNowNS;

                return new("DueDate can not be in the past!", dateNew < dateNowNS);
            };

            public Func<IState<TimeSpan>, bool> CanUpdateTime => s =>
            {
                return service.CanUpdateDueDate;
            };

            private static DateTime? NoSeconds(DateTime? dateTime)
            {
                if (dateTime.HasValue)
                {
                    return new(dateTime.Value.Year, dateTime.Value.Month, dateTime.Value.Day, dateTime.Value.Hour, 
                        dateTime.Value.Minute, 0, dateTime.Value.Kind);
                }

                return null;
            }
        }

        public IStateAsync<IDictionary<Guid, CRUDToDoItem>> CRUDItemDB { get; }
        public IStateGroup<DBRole> CRUDDBRoleGroup { get; }
        public CrudService()
        {
            CRUDItemDB = this.CreateStateAsync<IDictionary<Guid, CRUDToDoItem>>(new Dictionary<Guid, CRUDToDoItem>());
            CRUDDBRoleGroup = this.CreateStateGroup([DBRole.Admin, DBRole.User, DBRole.Guest], DBRole.Admin);
        }

        public CrudItemInput CreateItemInput(CRUDToDoItem? item = null)
        {
            return new CrudItemInput(this, item);
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
                await Task.Delay(200);
            };
        }

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> UpdateCRUDItemAsync(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                s.Value[item.Id!.Value] = item;
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanToggleCRUDItemCompleted => 
            _ => (CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User) && !CRUDItemDB.Changing();

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, CancellationToken, Task> ToggleCRUDItemCompletedAsync(CRUDToDoItem item)
        {
            return async (s, ct) =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                item = item with { Completed = !item.Completed };
                await Task.Delay(2000, ct);
                s.Value[item.Id!.Value] = item;
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanRemoveCRUDItem => _ =>
        {
            return !CRUDItemDB.Changing();
        };

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> RemoveCRUDItem(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                s.Value.Remove(item.Id!.Value);
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanRemoveCompletedCRUDItems => s =>
        {
            return s.Value.Values.Where(x => x.Completed).Any() &&
                (CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User) && !CRUDItemDB.Changing();
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
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, bool> CanRemoveAllCRUDItems => s =>
        {
            return s.Value.Values.Count != 0 && !CRUDItemDB.Changing();
        };

        public static Func<IStateAsync<IDictionary<Guid, CRUDToDoItem>>, Task> RemoveAllCRUDItems()
        {
            return async s =>
            {
                s.Value.Clear();
                await Task.Delay(200);
            };
        }

        private bool CanUpdateText => CRUDDBRoleGroup.Value is DBRole.Admin || CRUDDBRoleGroup.Value is DBRole.User;
        private bool CanUpdateDueDate => CRUDDBRoleGroup.Value is DBRole.Admin;
    }
}
