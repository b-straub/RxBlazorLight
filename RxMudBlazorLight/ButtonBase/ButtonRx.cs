using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    internal class ButtonRx<T> : ButtonBaseRx<T>
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

        public static ButtonRx<T> Create(MBButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent = null, bool hasProgress = false, string? cancelText = null, Color? cancelColor = null)
        {
            return new ButtonRx<T>(type, confirmExecutionAsync, buttonColor, buttonChildContent, hasProgress, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IState<T> state, Action<IState<T>> changeState, Func<IState<T>, bool>? canChange)
        {
            VerifyButtonParameters();
            
            Color = _buttonColor;
            if (_buttonType is not MBButtonType.FAB)
            {
                ChildContent = state.Changing() && _hasProgress ? RenderProgress() : _buttonChildContent;
            }

            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteState(state, changeState));
            OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => ExecuteState(state, changeState));

            Disabled = canChange is not null && !state.CanChange(canChange);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IStateAsync<T> state,
            Func<IStateAsync<T>, Task>? changeStateAsync,
            Func<IStateAsync<T>, CancellationToken, Task>? changeStateAsyncCancel,
            Func<IStateAsync<T>, bool>? canChange, bool deferredNotification)
        {
            VerifyButtonParametersAsync(changeStateAsync, changeStateAsyncCancel);

            if (state.Changing())
            {
                if (state.ChangeCallerID == _id)
                {
                    if (state.CanCancel && changeStateAsyncCancel is not null)
                    {
                        Color = _cancelColor ?? Color.Warning;

                        if (_buttonType is not MBButtonType.FAB)
                        {
                            ChildContent = RenderCancel();
                        }

                        OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, state.Cancel);
                        OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, state.Cancel);

                        Disabled = false;
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
                        ExecuteStateAsync(state, changeStateAsync, changeStateAsyncCancel, deferredNotification));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () =>
                        ExecuteStateAsync(state, changeStateAsync, changeStateAsyncCancel, deferredNotification));

                Disabled = canChange is not null && !state.CanChange(canChange);
            }

            OnClick ??= EventCallback.Factory.Create<MouseEventArgs>(this, _ => { });
            OnTouch ??= EventCallback.Factory.Create<TouchEventArgs>(this, _ => { });
        }

        private async Task ExecuteState(IState<T> state, Action<IState<T>> changeState)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {
                state.Change(changeState);
            }
        }

        private async Task ExecuteStateAsync(IStateAsync<T> state, Func<IStateAsync<T>, Task>? changeStateAsync,
            Func<IStateAsync<T>, CancellationToken, Task>? changeStateAsyncCancel, bool deferredNotification)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            { 
                if (changeStateAsync is not null)
                {   
                    await state.ChangeAsync(changeStateAsync, !deferredNotification, _id);
                }

                if (changeStateAsyncCancel is not null)
                {
                    await state.ChangeAsync(changeStateAsyncCancel, !deferredNotification, _id);
                }
            }
        }
    }
}
