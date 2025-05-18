
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
        ADMIN,
        USER,
        GUEST
    }

    public static class CrudServiceExtensions
    {
        public static bool CanAdd(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN;
        }

        public static bool CanUpdateText(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN;
        }

        public static bool CanUpdateDueDate(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN || roleGroup.Value is DBRole.USER;
        }

        public static bool CanUpdateCompleted(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN || roleGroup.Value is DBRole.USER;
        }

        public static bool CanDeleteCompleted(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN || roleGroup.Value is DBRole.USER;
        }

        public static bool CanDeleteOne(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN || roleGroup.Value is DBRole.USER;
        }

        public static bool CanDeleteAll(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.ADMIN;
        }
    }

    public class CrudService : RxBLService
    {
        public sealed class CrudItemInput : RxBLStateScope<CrudService>
        {
            private readonly CrudService _service;
            private readonly CRUDToDoItem? _item;

            public CrudItemInput(CrudService service, CRUDToDoItem? item) : base(service)
            {
                _service = service;
                _item = item;
                Text = this.CreateState(item is null ? string.Empty : item.Text);
                DueDateDate = this.CreateState(item is null ? DateTime.Now.Date : item.DueDate.Date);
                DueDateTime = this.CreateState(item is null ? DateTime.Now.TimeOfDay : item.DueDate.TimeOfDay);
            }

            public IState<string> Text { get; }
            public IState<DateTime> DueDateDate { get; }
            public IState<TimeSpan> DueDateTime { get; }

            public async Task SubmitAsync()
            {
                var newItem = new CRUDToDoItem(Text.Value, DueDateDate.Value + DueDateTime.Value, false, _item?.Id ?? Guid.NewGuid());
                await _service.CommandAsync.ExecuteAsync(_service.AddCRUDItem(newItem));
            }
            public bool CanSubmit()
            {
                var dateItem = NoSeconds(_item?.DueDate);
                var dateNew = NoSeconds(DueDateDate.Value.Date + DueDateTime.Value);

                return Text.Value != string.Empty && DueDateDate.Value.Date >= DateTime.Now.Date &&
                    (Text.Value != _item?.Text || dateNew != dateItem);
            }

            public Func<bool> CanUpdateText => () =>
            {
                return _service.CanUpdateText;
            };

            public Func<bool> CanUpdateDueDate => () =>
            {
                return _service.CanUpdateDueDate;
            };

            public static Func<string, StateValidation> ValidateText => v =>
            {
                return new("Text can not be empty!", v.Length == 0);
            };

            public static Func<DateTime, StateValidation> ValidateDueDate => v =>
            {
                return new("DueDate can not be in the past!", v.Date < DateTime.Now.Date);
            };

            public Func<TimeSpan, StateValidation> ValidateDueDateTime => v =>
            {
                var dateNowNs = NoSeconds(DateTime.Now);
                var dateNew = _item is null ? NoSeconds(DueDateDate.Value.Date + v) : dateNowNs;

                return new("DueDate can not be in the past!", dateNew < dateNowNs);
            };

            public Func<bool> CanUpdateTime => () =>
            {
                return _service.CanUpdateDueDate;
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

        public IEnumerable<CRUDToDoItem> CRUDItems => _db.Values;

        public IStateGroup<DBRole> CruddbRoleGroup { get; }

        private readonly Dictionary<Guid, CRUDToDoItem> _db;
        public CrudService()
        {
            _db = [];
            CruddbRoleGroup = this.CreateStateGroup([DBRole.ADMIN, DBRole.USER, DBRole.GUEST]);
        }

        public CrudItemInput CreateItemInput(CRUDToDoItem? item = null)
        {
            return new CrudItemInput(this, item);
        }

        public static Func<bool> CanChangeRole => () => true;

        public Func<bool> CanAdd => () => CruddbRoleGroup.CanAdd();
        public Func<bool> CanUpdate(CRUDToDoItem? item) => () => (CanUpdateText || CanUpdateDueDate) && !(item is not null && item.Completed);

        public Func<IStateCommandAsync, Task> AddCRUDItem(CRUDToDoItem item)
        {
            return async _ =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db[item.Id!.Value] = item;
                await Task.Delay(200);
            };
        }

        public Func<IStateCommandAsync, Task> UpdateCRUDItemAsync(CRUDToDoItem item)
        {
            return async _ =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db[item.Id!.Value] = item;
                await Task.Delay(200);
            };
        }

        public Func<bool> CanToggleCRUDItemCompleted => () => CruddbRoleGroup.CanUpdateCompleted();

        public Func<IStateCommandAsync, Task> ToggleCRUDItemCompletedAsync(CRUDToDoItem item)
        {
            return async c =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                item = item with { Completed = !item.Completed };
                await Task.Delay(2000, c.CancellationToken);
                _db[item.Id!.Value] = item;
            };
        }

        public Func<bool> CanRemoveCRUDItem => () => CruddbRoleGroup.CanDeleteOne();

        public Func<IStateCommandAsync, Task> RemoveCRUDItem(CRUDToDoItem item)
        {
            return async _ =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db.Remove(item.Id!.Value);
                await Task.Delay(200);
            };
        }

        public Func<bool> CanRemoveCompletedCRUDItems => () =>
        {
            return _db.Values.Where(x => x.Completed).Any() && CruddbRoleGroup.CanDeleteCompleted();
        };


        public Func<IStateCommandAsync, Task> RemoveCompletedCRUDItems()
        {
            return async _ =>
            {
                foreach (var item in _db.Values)
                {
                    if (item.Completed)
                    {
                        _db.Remove(item.Id!.Value);
                    }
                }
                await Task.Delay(200);
            };
        }

        public Func<bool> CanRemoveAllCRUDItems => () =>
        {
            return _db.Values.Count != 0 && CruddbRoleGroup.CanDeleteAll();
        };

        public Func<IStateCommandAsync, Task> RemoveAllCRUDItems()
        {
            return async _ =>
            {
                _db.Clear();
                await Task.Delay(200);
            };
        }

        private bool CanUpdateText => CruddbRoleGroup.CanUpdateText();
        private bool CanUpdateDueDate => CruddbRoleGroup.CanUpdateDueDate();
    }
}
