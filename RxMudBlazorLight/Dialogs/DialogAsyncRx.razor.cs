﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Buttons;
using RxMudBlazorLight.Extensions;

namespace RxMudBlazorLight.Dialogs
{
    public partial class DialogAsyncRx<T>
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
        public bool HasProgress { get; set; }

        private MudButtonAsyncRx? _buttonRef;
        private bool _closing;

        public static async Task<bool> Show(T owner, IDialogService dialogService,
            IStateCommandAsync stateCommand, Func<IStateCommandAsync, Task> executeAsyncCallback, string title,
            string message, string confirmButton, string cancelButton, bool successOnConfirm, string? cancelText = null,
            Color? cancelColor = null, bool hasProgress = true,
            Func<bool>? canChange = null)
        {
            var parameters = new DialogParameters
            {
                ["Owner"] = owner,
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

            var dialog = await dialogService.ShowAsync<DialogAsyncRx<T>>(title, parameters);

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

        protected override void OnServiceStateHasChanged(IList<ServiceChangeReason> crList)
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