﻿
using RxBlazorLightCore;
using System.Reactive;

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
        public sealed partial class CrudItemInput(CrudService service, CRUDToDoItem? item)
        {
            public IState<string> Text { get; } = service.CreateState(item is null ? string.Empty : item.Text);
            public IState<DateTime> DueDateDate { get; } = service.CreateState(item is null ? DateTime.Now.Date : item.DueDate.Date);
            public IState<TimeSpan> DueDateTime { get; } = service.CreateState(item is null ? DateTime.Now.TimeOfDay : item.DueDate.TimeOfDay);

            private readonly CRUDToDoItem? _item = item;

            public async Task SubmitAsync()
            {
                var newItem = new CRUDToDoItem(Text.Value, DueDateDate.Value + DueDateTime.Value, false, _item?.Id ?? Guid.NewGuid());
                await service.CRUDDBState.ChangeAsync(service.AddCRUDItem(newItem));
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

        public IEnumerable<CRUDToDoItem> CRUDItems => _db.Values;

        public IStateAsync<Unit> CRUDDBState { get; }
        public IStateGroup<DBRole> CRUDDBRoleGroup { get; }

        private readonly Dictionary<Guid, CRUDToDoItem> _db;
        public CrudService()
        {
            _db = [];
            CRUDDBState = this.CreateStateAsync(Unit.Default);
            CRUDDBRoleGroup = this.CreateStateGroup([DBRole.Admin, DBRole.User, DBRole.Guest], DBRole.Admin);
        }

        public CrudItemInput CreateItemInput(CRUDToDoItem? item = null)
        {
            return new CrudItemInput(this, item);
        }

        public Func<IState<DBRole>, bool> CanChangeRole => _ => !CRUDDBState.Changing();

        public bool CanAdd => !CRUDDBState.Changing() && CRUDDBRoleGroup.CanAdd();
        public bool CanUpdate => (CanUpdateText || CanUpdateDueDate) && !CRUDDBState.Changing();

        public Func<IStateAsync<Unit>, Task> AddCRUDItem(CRUDToDoItem item)
        {
            return async _ =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db[item.Id!.Value] = item;
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<Unit>, Task> UpdateCRUDItemAsync(CRUDToDoItem item)
        {
            return async _ =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db[item.Id!.Value] = item;
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<Unit>, bool> CanToggleCRUDItemCompleted => 
            _ => !CRUDDBState.Changing() && CRUDDBRoleGroup.CanUpdateCompleted();

        public Func<IStateAsync<Unit>, CancellationToken, Task> ToggleCRUDItemCompletedAsync(CRUDToDoItem item)
        {
            return async (_, ct) =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                item = item with { Completed = !item.Completed };
                await Task.Delay(2000, ct);
                _db[item.Id!.Value] = item;
            };
        }

        public Func<IStateAsync<Unit>, bool> CanRemoveCRUDItem => _ =>
        {
            return !CRUDDBState.Changing() && CRUDDBRoleGroup.CanDeleteOne();
        };

        public Func<IStateAsync<Unit>, Task> RemoveCRUDItem(CRUDToDoItem item)
        {
            return async s =>
            {
                ArgumentNullException.ThrowIfNull(item.Id, nameof(item.Id));
                _db.Remove(item.Id!.Value);
                await Task.Delay(200);
            };
        }

        public Func<IStateAsync<Unit>, bool> CanRemoveCompletedCRUDItems => s =>
        {
            return _db.Values.Where(x => x.Completed).Any() && !CRUDDBState.Changing() && CRUDDBRoleGroup.CanDeleteCompleted();
        };


        public Func<IStateAsync<Unit>, Task> RemoveCompletedCRUDItems()
        {
            return async s =>
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

        public Func<IStateAsync<Unit>, bool> CanRemoveAllCRUDItems => s =>
        {
            return _db.Values.Count != 0 && !CRUDDBState.Changing() && CRUDDBRoleGroup.CanDeleteAll();
        };

        public Func<IStateAsync<Unit>, Task> RemoveAllCRUDItems()
        {
            return async s =>
            {
                _db.Clear();
                await Task.Delay(200);
            };
        }

        private bool CanUpdateText => CRUDDBRoleGroup.CanUpdateText();
        private bool CanUpdateDueDate => CRUDDBRoleGroup.CanUpdateDueDate();
    }
}
