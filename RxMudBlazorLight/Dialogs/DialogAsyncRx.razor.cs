using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncRx : ComponentBase
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
        public required ICommandAsync RxCommandAsync { get; init; }

        [Parameter]
        public Func<ICommandAsync, CancellationToken, Task<bool>>? PrepareExecutionAsync { get; set; }

        [Parameter]
        public Action? BeforeExecution { get; set; }

        [Parameter]
        public Action? AfterExecution { get; set; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        private MudButtonAsyncRx? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService DialogService,
           ICommandAsync RxCommandAsync, string title,
           string message, string confirmButton, string cancelButton, bool successOnConfirm = false,
           string? cancelText = null, Color? cancelColor = null)
        {
            var parameters = new DialogParameters
            {
                ["RxCommandAsync"] = RxCommandAsync,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm,
                ["CancelColor"] = cancelColor,
                ["CancelText"] = cancelText
            };

            var dialog = DialogService.Show<DialogAsyncRx>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        private bool CanNotCancel()
        {
            if (_buttonRef?.RxCommandAsync is null)
            {
                return false;
            }

            return _buttonRef.RxCommandAsync.Running;
        }

        private void Cancel()
        {
            ArgumentNullException.ThrowIfNull(_buttonRef?.RxCommandAsync);

            if (!_buttonRef.RxCommandAsync.Running)
            {
                MudDialog?.Cancel();
            }
        }

        private void BeforeExecutionDo()
        {
            _canceled = false;

            if (_buttonRef?.RxCommandAsync is not null && _buttonDisposable is null)
            {
                _buttonDisposable = _buttonRef.RxCommandAsync.Subscribe(cs =>
                {
                    InvokeAsync(StateHasChanged);
                     _canceled = cs is CommandState.CANCELED;
                });
            }

            if (BeforeExecution is not null)
            {
                BeforeExecution();
            }
        }

        private void AfterExecutionDo()
        {
            if (AfterExecution is not null)
            {
                AfterExecution();
            }

            if (!_canceled)
            {
                _buttonDisposable?.Dispose();
                _buttonDisposable = null;

                MudDialog?.Close(DialogResult.Ok(true));
            }
        }
    }
}
