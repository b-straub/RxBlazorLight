using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;

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
        public required IStateProvider<TParam> StateProvider { get; init; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        [Parameter]
        public Func<bool>? DisabledFactory { get; set; }

        private MudButtonRx<TParam>? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService dialogService,
           IStateProvider<TParam> stateProvider, string title,
           string message, string confirmButton, string cancelButton, bool successOnConfirm,
           string? cancelText = null, Color? cancelColor = null, Func<bool>? disabledFactory = null)
        {
            var parameters = new DialogParameters
            {
                ["StateProvider"] = stateProvider,
                ["CancelColor"] = cancelColor,
                ["CancelText"] = cancelText,
                ["DisabledFactory"] = disabledFactory,
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
            if (_buttonRef?.StateProvider is null)
            {
                return false;
            }

            return _buttonRef.StateProvider.Changing();
        }

        private void Cancel()
        {
            ArgumentNullException.ThrowIfNull(_buttonRef?.StateProvider);

            if (!_buttonRef.StateProvider.Changing())
            {
                MudDialog?.Cancel();
            }
        }

        protected override void ServiceStateHasChanged(Guid id, ChangeReason changeReason)
        {
            if (changeReason is ChangeReason.STATE && _buttonRef is not null && id == _buttonRef.StateProvider.ID)
            {
                if (_buttonRef.StateProvider.Done())
                {
                    _canceled = StateProvider.Phase is StateChangePhase.CANCELED;
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
