using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.ButtonBase;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncRx<T>
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
        public required IStateCommandAsync StateCommand { get; init; }

        [Parameter]
        public Func<Task>? ChangeStateAsync { get; init; }

        [Parameter]
        public Func<CancellationToken, Task>? ChangeStateAsyncCancel { get; init; }

        [Parameter]
        public Func<bool>? CanChange { get; init; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        [Parameter]
        public bool HasProgress { get; set; } = true;

        private MudButtonAsyncBaseRx? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService dialogService,
           IStateCommandAsync stateCommand, Func<Task>? changeStateAsync, string title,
           string message, string confirmButton, string cancelButton, bool successOnConfirm, bool hasProgress = true,
           Func<bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["StateCommand"] = stateCommand,
                ["ChangeStateAsync"] = changeStateAsync,
                ["CanChange"] = canChange,
                ["HasProgress"] = hasProgress,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = dialogService.Show<DialogAsyncRx<T>>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> Show(IDialogService dialogService,
          IStateCommandAsync stateCommand, Func<CancellationToken, Task>? changeStateAsyncCancel, string title,
          string message, string confirmButton, string cancelButton, bool successOnConfirm, string cancelText, Color? cancelColor = null, bool hasProgress = true,
          Func<bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["StateCommand"] = stateCommand,
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

            var dialog = dialogService.Show<DialogAsyncRx<T>>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        private bool CanNotCancel()
        {
            if (_buttonRef?.StateCommand is null)
            {
                return false;
            }

            return _buttonRef.StateCommand.Changing();
        }

        private void Cancel()
        {
            ArgumentNullException.ThrowIfNull(_buttonRef?.StateCommand);

            if (!_buttonRef.StateCommand.Changing())
            {
                MudDialog?.Cancel();
            }
        }

        protected override void OnServiceStateHasChanged(ServiceChangeReason cr)
        {
            if (cr.Reason is ChangeReason.STATE && _buttonRef is not null && cr.StateID == _buttonRef.StateCommand.ID)
            {
                if (_buttonRef.StateCommand.Done())
                {
                    _canceled = StateCommand.Canceled();
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
