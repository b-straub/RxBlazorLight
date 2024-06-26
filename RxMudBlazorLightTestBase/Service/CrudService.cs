﻿
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

    public static class CrudServiceExtensions
    {
        public static bool CanAdd(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin;
        }

        public static bool CanUpdateText(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin;
        }

        public static bool CanUpdateDueDate(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin || roleGroup.Value is DBRole.User;
        }

        public static bool CanUpdateCompleted(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin || roleGroup.Value is DBRole.User;
        }

        public static bool CanDeleteCompleted(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin || roleGroup.Value is DBRole.User;
        }

        public static bool CanDeleteOne(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin || roleGroup.Value is DBRole.User;
        }

        public static bool CanDeleteAll(this IStateGroup<DBRole> roleGroup)
        {
            return roleGroup.Value is DBRole.Admin;
        }
    }

    public class CrudService : RxBLService
    {
        public sealed class CrudItemInput(CrudService service, CRUDToDoItem? item)
        {
            public IState<string> Text { get; } = service.CreateState(item is null ? string.Empty : item.Text);
            public IState<DateTime> DueDateDate { get; } = service.CreateState(item is null ? DateTime.Now.Date : item.DueDate.Date);
            public IState<TimeSpan> DueDateTime { get; } = service.CreateState(item is null ? DateTime.Now.TimeOfDay : item.DueDate.TimeOfDay);

            public async Task SubmitAsync()
            {
                var newItem = new CRUDToDoItem(Text.Value, DueDateDate.Value + DueDateTime.Value, false, item?.Id ?? Guid.NewGuid());
                await service.CommandAsync.ExecuteAsync(service.AddCRUDItem(newItem));
            }
            public bool CanSubmit()
            {
                var dateItem = NoSeconds(item?.DueDate);
                var dateNew = NoSeconds(DueDateDate.Value.Date + DueDateTime.Value);

                return Text.Value != string.Empty && DueDateDate.Value.Date >= DateTime.Now.Date &&
                    (Text.Value != item?.Text || dateNew != dateItem);
            }

            public Func<bool> CanUpdateText => () =>
            {
                return service.CanUpdateText;
            };

            public Func<bool> CanUpdateDueDate => () =>
            {
                return service.CanUpdateDueDate;
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
                var dateNowNS = NoSeconds(DateTime.Now);
                var dateNew = item is null ? NoSeconds(DueDateDate.Value.Date + v) : dateNowNS;

                return new("DueDate can not be in the past!", dateNew < dateNowNS);
            };

            public Func<bool> CanUpdateTime => () =>
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

        public IEnumerable<CRUDToDoItem> CRUDItems => _db.Values;

        public IStateGroup<DBRole> CRUDDBRoleGroup { get; }

        private readonly Dictionary<Guid, CRUDToDoItem> _db;
        public CrudService()
        {
            _db = [];
            CRUDDBRoleGroup = this.CreateStateGroup([DBRole.Admin, DBRole.User, DBRole.Guest], DBRole.Admin);
        }

        public CrudItemInput CreateItemInput(CRUDToDoItem? item = null)
        {
            return new CrudItemInput(this, item);
        }

        public static Func<bool> CanChangeRole => () => true;

        public Func<bool> CanAdd => () => CRUDDBRoleGroup.CanAdd();
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

        public Func<bool> CanToggleCRUDItemCompleted => () => CRUDDBRoleGroup.CanUpdateCompleted();

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

        public Func<bool> CanRemoveCRUDItem => () => CRUDDBRoleGroup.CanDeleteOne();

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
            return _db.Values.Where(x => x.Completed).Any() && CRUDDBRoleGroup.CanDeleteCompleted();
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
            return _db.Values.Count != 0 && CRUDDBRoleGroup.CanDeleteAll();
        };

        public Func<IStateCommandAsync, Task> RemoveAllCRUDItems()
        {
            return async _ =>
            {
                _db.Clear();
                await Task.Delay(200);
            };
        }

        private bool CanUpdateText => CRUDDBRoleGroup.CanUpdateText();
        private bool CanUpdateDueDate => CRUDDBRoleGroup.CanUpdateDueDate();
    }
}
