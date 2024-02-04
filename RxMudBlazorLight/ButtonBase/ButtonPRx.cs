using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

namespace RxMudBlazorLight.ButtonBase
{
    internal class ButtonPRx<T> : ButtonBaseRx<T>
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }
        public EventCallback<TouchEventArgs>? OnTouch { get; private set; }

        private readonly Func<IStateTransformer<T>, Task> _valueFactoryAsync;

        private ButtonPRx(MBButtonType type, Func<IStateTransformer<T>, Task> valueFactoryAsync, Color buttonColor,
            RenderFragment? buttonChildContent, string? cancelText, Color? cancelColor) :
            base(type, buttonColor, buttonChildContent, cancelText, cancelColor)
        {
            _valueFactoryAsync = valueFactoryAsync;
        }

        public static ButtonPRx<T> Create(MBButtonType type, Func<IStateTransformer<T>, Task> valueFactoryAsync, Color buttonColor,
            RenderFragment ? buttonChildContent = null, string? cancelText = null, Color? cancelColor = null)
        {
            return new ButtonPRx<T>(type, valueFactoryAsync, buttonColor, buttonChildContent, cancelText, cancelColor);
        }

        [MemberNotNull(nameof(OnClick))]
        [MemberNotNull(nameof(OnTouch))]
        public void SetParameter(IStateTransformer<T> stateTransformer)
        {
            VerifyCancelArguments(stateTransformer.CanCancel);

            if (stateTransformer.Changing() && stateTransformer.CanCancel)
            {
                Color = _cancelColor ?? Color.Warning;

                if (_buttonType is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => stateTransformer.Cancel());
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => stateTransformer.Cancel());

                Disabled = false;
            }
            else
            {
                Color = _buttonColor;
                if (_buttonType is not MBButtonType.FAB)
                {
                    ChildContent = stateTransformer.Changing() && stateTransformer.LongRunning ? RenderProgress() : _buttonChildContent;
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteValueProvider(stateTransformer));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => ExecuteValueProvider(stateTransformer));

                Disabled = !stateTransformer.CanRun;
            }
        }

        private async Task ExecuteValueProvider(IStateTransformer<T> stateTransformer)
        {
            await _valueFactoryAsync(stateTransformer);
        }
    }
}
