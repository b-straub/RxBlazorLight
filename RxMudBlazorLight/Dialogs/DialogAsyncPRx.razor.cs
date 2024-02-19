using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncPRx<TService, TParam> : RxBLServiceChangeSubscriber<TService> where TService : IRxBLService
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
        public required IStateTransformer<TParam> StateTransformer { get; init; }

        [Parameter, EditorRequired]
        public required Func<IStateTransformer<TParam>, Task> ValueFactoryAsync { get; init; }

        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        [Parameter]
        public TParam? Context { get; set; }

        private MudButtonPRx<TParam>? _buttonRef;
        private IDisposable? _buttonDisposable;
        private bool _canceled = false;

        public static async Task<bool> Show(IDialogService dialogService,
            IStateTransformer<TParam> stateTransformer, Func<IStateTransformer<TParam>, Task> valueFactoryAsync, string title,
            string message, string confirmButton, string cancelButton, bool successOnConfirm,
            string? cancelText = null, Color? cancelColor = null, TParam? context = default)
        {
            var parameters = new DialogParameters
            {
                ["StateTransformer"] = stateTransformer,
                ["ValueFactoryAsync"] = valueFactoryAsync,
                ["CancelText"] = cancelText,
                ["CancelColor"] = cancelColor,
                ["Context"] = context,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            var dialog = dialogService.Show<DialogAsyncPRx<TService, TParam>>(title, parameters);

            var res = await dialog.Result;

            if (res.Canceled)
            {
                return false;
            }

            return true;
        }

        private bool CanNotCancel()
        {
            if (_buttonRef?.StateTransformer is null)
            {
                return false;
            }

            return _buttonRef.StateTransformer.Changing();
        }

        private void Cancel()
        {
            ArgumentNullException.ThrowIfNull(_buttonRef?.StateTransformer);

            if (!_buttonRef.StateTransformer.Changing())
            {
                MudDialog?.Cancel();
            }
        }

        protected override void ServiceStateHasChanged(Guid id, ChangeReason changeReason)
        {
            if (changeReason is ChangeReason.STATE && _buttonRef is not null && id == _buttonRef.StateTransformer.ID)
            {
                if (_buttonRef.StateTransformer.Done() )
                {
                    _canceled = StateTransformer.Phase is StateChangePhase.CANCELED;
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
