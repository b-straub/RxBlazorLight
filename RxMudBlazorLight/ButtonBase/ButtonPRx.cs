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
        private readonly Func<bool>? _disabledFactory;

        private ButtonPRx(MBButtonType type, Func<IStateTransformer<T>, Task> valueFactoryAsync, Color buttonColor,
            RenderFragment? buttonChildContent, string? cancelText, Color? cancelColor, Func<bool>? disabledFactory) :
            base(type, buttonColor, buttonChildContent, cancelText, cancelColor)
        {
            _valueFactoryAsync = valueFactoryAsync;
            _disabledFactory = disabledFactory;
        }

        public static ButtonPRx<T> Create(MBButtonType type, Func<IStateTransformer<T>, Task> valueFactoryAsync, Color buttonColor,
            RenderFragment ? buttonChildContent = null, string? cancelText = null, Color? cancelColor = null, Func<bool>? disabledFactory = null)
        {
            return new ButtonPRx<T>(type, valueFactoryAsync, buttonColor, buttonChildContent, cancelText, cancelColor, disabledFactory);
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

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => ExecuteStateProvider(stateTransformer));
                OnTouch = EventCallback.Factory.Create<TouchEventArgs>(this, () => ExecuteStateProvider(stateTransformer));

                Disabled = _disabledFactory is null ? !stateTransformer.CanRun : _disabledFactory();
            }
        }

        private async Task ExecuteStateProvider(IStateTransformer<T> stateTransformer)
        {
            await _valueFactoryAsync(stateTransformer);
        }
    }
}
