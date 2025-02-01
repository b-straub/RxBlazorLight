using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;
using RxMudBlazorLight.Extensions;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogRx<T>
    {
        [CascadingParameter]
        IMudDialogInstance? MudDialog { get; set; }

        [Parameter, EditorRequired]
        public required string Message { get; init; }

        [Parameter, EditorRequired]
        public required string ConfirmButton { get; set; }

        [Parameter, EditorRequired]
        public required string CancelButton { get; set; }

        [Parameter]
        public bool SuccessOnConfirm { get; set; }

        [Parameter, EditorRequired]
        public required IStateCommand StateCommand { get; init; }

        [Parameter, EditorRequired]
        public required Action ExecuteCallback { get; init; }

        [Parameter]
        public Func<bool>? CanChangeCallback { get; init; }

        private MudButtonRx? _buttonRef;
        private bool _closing;

        public static async Task<bool> Show(IDialogService dialogService,
            IStateCommand stateCommand, Action executeCallback, string title,
            string message, string confirmButton, string cancelButton, bool successOnConfirm,
            Func<bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["StateCommand"] = stateCommand,
                ["ExecuteCallback"] = executeCallback,
                ["CanChangeCallback"] = canChange,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = await dialogService.ShowAsync<DialogRx<T>>(title, parameters);

            var res = await dialog.Result;
            return res.OK();
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

            if (!crList.Contains(_buttonRef.StateCommand) || !_buttonRef.StateCommand.Done() || StateCommand.Canceled())
            {
                return;
            }

            _closing = true;
            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}