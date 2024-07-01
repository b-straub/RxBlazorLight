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

        private ButtonRx(MBButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent, bool hasProgress, string? cancelText, Color? cancelColor) :
            base(type, buttonColor, buttonChildContent, hasProgress, cancelText, cancelColor)
        {
            _confirmExecutionAsync = confirmExecutionAsync;
        }

        public static ButtonRx Create(MBButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent = null, bool hasProgress = false, string? cancelText = null, Color? cancelColor = null)
        {
            return new ButtonRx(type, confirmExecutionAsync, buttonColor, buttonChildContent, hasProgress, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameter(IStateCommand stateCommand, Action executeCallback, Func<bool>? canChangeCallback)
        {
            VerifyButtonParameters();

            if (_buttonType is not MBButtonType.FAB)
            {
                ChildContent = stateCommand.Changing() && _hasProgress ? RenderProgress() : _buttonChildContent;
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
                if (stateCommand.ChangeCallerID == _id)
                {
                    if (stateCommand.CanCancel)
                    {
                        Color = _cancelColor ?? Color.Warning;

                        if (_buttonType is not MBButtonType.FAB)
                        {
                            ChildContent = RenderCancel();
                        }

                        OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, stateCommand.Cancel);

                        if (_buttonType is MBButtonType.ICON)
                        {
                            Disabled = true;
                        }
                        else if (_buttonType is MBButtonType.FAB && _cancelText is null)
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
                        if (_buttonType is not MBButtonType.FAB && _hasProgress)
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
                ChildContent = _buttonChildContent;
                Color = _buttonColor;

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
                await stateCommand.ExecuteAsync(executeAsyncCallback, deferredNotification, _id);
            }
        }
    }
}
