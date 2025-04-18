using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncORx<T>
    {
        [CascadingParameter]
        public IMudDialogInstance? MudDialog { get; set; }

        [Parameter, EditorRequired]
        public required string Message { get; init; }

        [Parameter, EditorRequired]
        public required string ConfirmButton { get; set; }

        [Parameter, EditorRequired]
        public required string CancelButton { get; set; }

        [Parameter]
        public bool SuccessOnConfirm { get; set; }

        [Parameter, EditorRequired]
        public required IStateProgressObserverAsync ProgressStateObserver { get; init; }

        [Parameter, EditorRequired]
        public required Func<IStateProgressObserverAsync, IDisposable> ExecuteAsyncCallback { get; init; }
    
        [Parameter]
        public string? CancelText { get; set; }

        [Parameter]
        public Color? CancelColor { get; set; }

        private MudButton? _buttonRef;
        private bool _closing;

        public static async Task<bool> Show(IDialogService dialogService,
            IStateProgressObserverAsync progressStateObserver, Func<IStateProgressObserverAsync, IDisposable> executeAsyncCallback, string title,
            string message, string confirmButton, string cancelButton, bool successOnConfirm, string? cancelText = null,
            Color? cancelColor = null)
        {
            var parameters = new DialogParameters
            {
                ["ProgressStateObserver"] = progressStateObserver,
                ["ExecuteAsyncCallback"] = executeAsyncCallback,
                ["CancelColor"] = cancelColor,
                ["CancelText"] = cancelText,
                ["Message"] = message,
                ["ConfirmButton"] = confirmButton,
                ["CancelButton"] = cancelButton,
                ["SuccessOnConfirm"] = successOnConfirm
            };

            progressStateObserver.ResetException();
            var dialog = await dialogService.ShowAsync<DialogAsyncORx<T>>(title, parameters);

            var res = await dialog.Result;
            return res.OK();
        }

        private bool CanNotCancel()
        {
            return ProgressStateObserver.Changing();
        }

        private void Cancel()
        {
            if (!ProgressStateObserver.Changing())
            {
                MudDialog?.Cancel();
            }
        }

        protected override void OnServiceStateHasChanged(IList<ServiceChangeReason> crList)
        {
            if (_closing || _buttonRef is null || !ProgressStateObserver.Done() || ProgressStateObserver.Canceled())
            {
                return;
            }

            if (!crList.Contains(ProgressStateObserver) || ProgressStateObserver.Phase is StatePhase.EXCEPTION)
            {
                return;
            }

            _closing = true;
            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}