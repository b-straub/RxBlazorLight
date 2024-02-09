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
        private readonly Func<bool>? _disabledFactory;

        private ButtonRx(MBButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor, 
            RenderFragment? buttonChildContent, string? cancelText, Color? cancelColor, Func<bool>? disabledFactory) :
            base(type, buttonColor, buttonChildContent, cancelText, cancelColor)
        {
            _confirmExecutionAsync = confirmExecutionAsync;
            _disabledFactory = disabledFactory;
        }

        public static ButtonRx<T> Create(MBButtonType type, Func<Task<bool>>? confirmExecutionAsync, Color buttonColor,
            RenderFragment? buttonChildContent = null, string? cancelText = null, Color? cancelColor = null, Func<bool>? disabledFactory = null)
        {
            return new ButtonRx<T>(type, confirmExecutionAsync, buttonColor, buttonChildContent, cancelText, cancelColor, disabledFactory);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IStateProvider<T> stateProvider)
        {
            VerifyCancelArguments(stateProvider.CanCancel);

            if (stateProvider.Changing() && stateProvider.CanCancel)
            {
                Color = _cancelColor ?? Color.Warning;
;
                if (_buttonType is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => stateProvider.Cancel());
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => stateProvider.Cancel());

                Disabled = false;
            }
            else
            {
                Color = _buttonColor;
                if (_buttonType is not MBButtonType.FAB)
                {
                    ChildContent = stateProvider.Changing() && stateProvider.LongRunning ? RenderProgress() : _buttonChildContent;
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteStateProvider(stateProvider));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => ExecuteStateProvider(stateProvider));

                Disabled = _disabledFactory is null ? !stateProvider.CanRun : _disabledFactory();
            }
        }

        private async Task ExecuteStateProvider(IStateProvider<T> stateProvider)
        {
            if (_confirmExecutionAsync is null || await _confirmExecutionAsync())
            {
                stateProvider.Provide();
            }
        }
    }
}
