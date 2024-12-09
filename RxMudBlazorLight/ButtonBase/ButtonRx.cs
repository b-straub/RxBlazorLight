using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    internal class ButtonRx : ButtonBaseRx
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }

        private readonly Func<Task<bool>>? _confirmExecutionAsync;

        private ButtonRx(MbButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent, bool hasProgress, string? cancelText, Color? cancelColor) :
            base(type, buttonColor, buttonChildContent, hasProgress, cancelText, cancelColor)
        {
            _confirmExecutionAsync = confirmExecutionAsync;
        }

        public static ButtonRx Create(MbButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent = null, bool hasProgress = false, string? cancelText = null, Color? cancelColor = null)
        {
            return new ButtonRx(type, confirmExecutionAsync, buttonColor, buttonChildContent, hasProgress, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameter(IStateCommand stateCommand, Action executeCallback, Func<bool>? canChangeCallback)
        {
            VerifyButtonParameters();

            if (ButtonType is not MbButtonType.FAB)
            {
                ChildContent = stateCommand.Changing() && HasProgress ? RenderProgress() : ButtonChildContent;
            }

            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteStateCommand(stateCommand, executeCallback));
            Disabled = stateCommand.Disabled || (canChangeCallback is not null && !canChangeCallback());
        }

        private async Task ExecuteStateCommand(IStateCommand stateCommand, Action executeCallback)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {
                stateCommand.Execute(executeCallback);
            }
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameter(IStateCommandAsync stateCommand, Func<IStateCommandAsync, Task> executeAsyncCallback, Func<bool>? canChangeCallback, bool deferredNotification)
        {
            VerifyButtonParametersAsync(stateCommand);

            if (stateCommand.Changing())
            {
                if (stateCommand.ChangeCallerID == ID)
                {
                    if (stateCommand.CanCancel)
                    {
                        Color = CancelColor ?? Color.Warning;

                        if (ButtonType is not MbButtonType.FAB)
                        {
                            ChildContent = RenderCancel();
                        }

                        OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, stateCommand.Cancel);

                        if (ButtonType is MbButtonType.ICON)
                        {
                            Disabled = true;
                        }
                        else if (ButtonType is MbButtonType.FAB && CancelText is null)
                        {
                            Disabled = true;
                        }
                        else
                        {
                            Disabled = false;
                        }
                    }
                    else
                    {
                        if (ButtonType is not MbButtonType.FAB && HasProgress)
                        {
                            ChildContent = RenderProgress();
                        }

                        Disabled = true;
                    }
                }
                else
                {
                    Disabled = true;
                }
            }
            else
            {
                ChildContent = ButtonChildContent;
                Color = ButtonColor;

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () =>
                        ExecuteStateCommandAsync(stateCommand, executeAsyncCallback, deferredNotification));
               
                Disabled = stateCommand.Disabled || (canChangeCallback is not null && !canChangeCallback());
            }

            OnClick ??= EventCallback.Factory.Create<MouseEventArgs>(this, _ => { });
        }

        private async Task ExecuteStateCommandAsync(IStateCommandAsync stateCommand, Func<IStateCommandAsync, Task> executeAsyncCallback, bool deferredNotification)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {   
                await stateCommand.ExecuteAsync(executeAsyncCallback, deferredNotification, ID);
            }
        }
    }
}
