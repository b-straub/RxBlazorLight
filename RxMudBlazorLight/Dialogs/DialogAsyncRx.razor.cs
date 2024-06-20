using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncRx<T>
    {
        [CascadingParameter]
        public MudDialogInstance? MudDialog { get; set; }

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

        [Parameter, EditorRequired]
        public required Func<IStateCommandAsync, Task> ExecuteAsyncCallback { get; init; }

        [Parameter]
        public Func<bool>? CanChangeCallback { get; init; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        [Parameter]
        public bool HasProgress { get; set; } = false;

        private MudButtonAsyncRx? _buttonRef;
        private bool _closing;

        public static async Task<bool> Show(IDialogService dialogService,
            IStateCommandAsync stateCommand, Func<IStateCommandAsync, Task> executeAsyncCallback, string title,
            string message, string confirmButton, string cancelButton, bool successOnConfirm, string? cancelText = null,
            Color? cancelColor = null, bool hasProgress = true,
            Func<bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["StateCommand"] = stateCommand,
                ["ExecuteAsyncCallback"] = executeAsyncCallback,
                ["CanChangeCallback"] = canChange,
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

        protected override void OnServiceStateHasChanged(IEnumerable<ServiceChangeReason> crList)
        {
            if (_closing || _buttonRef is null || !_buttonRef.StateCommand.Done() || StateCommand.Canceled())
            {
                return;
            }

            if (!crList.Contains(_buttonRef.StateCommand))
            {
                return;
            }

            _closing = true;
            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}