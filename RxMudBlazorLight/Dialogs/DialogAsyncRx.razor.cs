using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.ButtonBase;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncRx<TService, TParam> : RxBLServiceChangeSubscriber<TService> where TService : IRxBLService
    {
        [CascadingParameter]
        MudDialogInstance? MudDialog { get; set; }

        [Parameter, EditorRequired]
        public required string Message { get; init; }

        [Parameter, EditorRequired]
        public required string ConfirmButton { get; set; }

        [Parameter, EditorRequired]
        public required string CancelButton { get; set; }

        [Parameter]
        public bool SuccessOnConfirm { get; set; } = false;

        [Parameter, EditorRequired]
        public required IStateAsync<TParam> State { get; init; }

        [Parameter]
        public Func<IStateAsync<TParam>, Task>? ChangeStateAsync { get; init; }

        [Parameter]
        public Func<IStateAsync<TParam>, CancellationToken, Task>? ChangeStateAsyncCancel { get; init; }

        [Parameter]
        public Func<IStateBase<TParam>, bool>? CanChange { get; init; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        [Parameter]
        public bool HasProgress { get; set; } = true;

        private MudButtonAsyncBaseRx<TParam>? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService dialogService,
           IStateAsync<TParam> state, Func<IStateAsync<TParam>, Task>? changeStateAsync, string title,
           string message, string confirmButton, string cancelButton, bool successOnConfirm, bool hasProgress = true,
           Func<IStateAsync<TParam>, bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["State"] = state,
                ["ChangeStateAsync"] = changeStateAsync,
                ["CanChange"] = canChange,
                ["HasProgress"] = hasProgress,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = dialogService.Show<DialogAsyncRx<TService, TParam>>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> Show(IDialogService dialogService,
          IStateAsync<TParam> state, Func<IStateAsync<TParam>, CancellationToken, Task>? changeStateAsyncCancel, string title,
          string message, string confirmButton, string cancelButton, bool successOnConfirm, string cancelText, Color? cancelColor = null, bool hasProgress = true,
          Func<IStateAsync<TParam>, bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["State"] = state,
                ["ChangeStateAsyncCancel"] = changeStateAsyncCancel,
                ["CanChange"] = canChange,
                ["CancelColor"] = cancelColor,
                ["CancelText"] = cancelText,
                ["HasProgress"] = hasProgress,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = dialogService.Show<DialogAsyncRx<TService, TParam>>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        private bool CanNotCancel()
        {
            if (_buttonRef?.State is null)
            {
                return false;
            }

            return _buttonRef.State.Changing();
        }

        private void Cancel()
        {
            ArgumentNullException.ThrowIfNull(_buttonRef?.State);

            if (!_buttonRef.State.Changing())
            {
                MudDialog?.Cancel();
            }
        }

        protected override void ServiceStateHasChanged(Guid id, ChangeReason changeReason)
        {
            if (changeReason is ChangeReason.STATE && _buttonRef is not null && id == _buttonRef.State.ID)
            {
                if (_buttonRef.State.Done())
                {
                    _canceled = State.Canceled();
                    if (!_canceled)
                    {
                        _buttonDisposable?.Dispose();
                        _buttonDisposable = null;

                        MudDialog?.Close(DialogResult.Ok(true));
                    }
                }
            }
        }
    }
}
