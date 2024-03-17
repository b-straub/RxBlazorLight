using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogRx<TService, TParam> : RxBLServiceChangeSubscriber<TService> where TService : IRxBLService
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
        public required IState<TParam> State { get; init; }

        [Parameter]
        public Func<IState<TParam>, Task>? ChangeStateAsync { get; init; }

        [Parameter]
        public Action<IState<TParam>>? ChangeState { get; init; }

        [Parameter]
        public Func<IState<TParam>, bool>? CanChange { get; init; }

        [Parameter]
        public bool HasProgress { get; set; } = false;

        private MudButtonRx<TParam>? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService dialogService,
           IState<TParam> state, Action<IState<TParam>> changeState, string title,
           string message, string confirmButton, string cancelButton, bool successOnConfirm,
           Func<IState<TParam>, bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["State"] = state,
                ["ChangeState"] = changeState,
                ["CanChange"] = canChange,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = dialogService.Show<DialogRx<TService, TParam>>(title, parameters);

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
