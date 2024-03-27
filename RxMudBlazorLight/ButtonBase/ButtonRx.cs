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
        public EventCallback<TouchEventArgs>? OnTouch { get; private set; }

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
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IStateCommand stateCommand, Action changeState, Func<bool>? canChange)
        {
            VerifyButtonParameters();

            Color = _buttonColor;
            if (_buttonType is not MBButtonType.FAB)
            {
                ChildContent = stateCommand.Changing() && _hasProgress ? RenderProgress() : _buttonChildContent;
            }

            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteState(stateCommand, changeState));
            OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => ExecuteState(stateCommand, changeState));

            Disabled = canChange is not null && !canChange();
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IStateCommandAsync stateCommand,
            Func<Task>? changeStateAsync,
            Func<CancellationToken, Task>? changeStateAsyncCancel,
            Func<bool>? canChange, bool deferredNotification)
        {
            VerifyButtonParametersAsync(changeStateAsync, changeStateAsyncCancel);

            if (stateCommand.Changing())
            {
                if (stateCommand.ChangeCallerID == _id)
                {
                    if (changeStateAsyncCancel is not null)
                    {
                        Color = _cancelColor ?? Color.Warning;

                        if (_buttonType is not MBButtonType.FAB)
                        {
                            ChildContent = RenderCancel();
                        }

                        OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, stateCommand.Cancel);
                        OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, stateCommand.Cancel);

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
                        ExecuteStateAsync(stateCommand, changeStateAsync, changeStateAsyncCancel, deferredNotification));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () =>
                        ExecuteStateAsync(stateCommand, changeStateAsync, changeStateAsyncCancel, deferredNotification));

                Disabled = canChange is not null && !canChange();
            }

            OnClick ??= EventCallback.Factory.Create<MouseEventArgs>(this, _ => { });
            OnTouch ??= EventCallback.Factory.Create<TouchEventArgs>(this, _ => { });
        }

        private async Task ExecuteState(IStateCommand stateCommand, Action changeState)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {
                stateCommand.Change(changeState);
            }
        }

        private async Task ExecuteStateAsync(IStateCommandAsync stateCommand, Func<Task>? changeStateAsync,
            Func<CancellationToken, Task>? changeStateAsyncCancel, bool deferredNotification)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {
                if (changeStateAsync is not null)
                {
                    await stateCommand.ChangeAsync(changeStateAsync, _hasProgress, _id);
                }

                if (changeStateAsyncCancel is not null)
                {
                    await stateCommand.ChangeAsync(changeStateAsyncCancel, !deferredNotification, _id);
                }
            }
        }
    }
}
