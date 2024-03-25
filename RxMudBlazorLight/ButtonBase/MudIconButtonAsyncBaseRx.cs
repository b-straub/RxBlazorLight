using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class MudIconButtonAsyncBaseRx<T> : MudIconButton
    {
        [Parameter, EditorRequired]
        public required IStateAsync<T> State { get; init; }

        [Parameter]
        public Func<IStateAsync<T>, bool>? CanChange { get; init; }

        [Parameter]
        public Func<Task<bool>>? ConfirmExecutionAsync { get; init; }

        [Parameter]
        public MBIconVariant? IconVariant { get; set; }

        protected Func<IStateAsync<T>, Task>? _changeStateAsync;
        protected Func<IStateAsync<T>, CancellationToken, Task>? _changeStateAsyncCancel;
        protected string? _cancelText;
        protected Color? _cancelColor;
        protected bool _hasProgress = false;
        protected bool _deferredNotification = false;
        protected RenderFragment RenderBase() => base.BuildRenderTree;

        internal ButtonRx<T>? _buttonRx;

        protected override void OnInitialized()
        {
            _buttonRx = ButtonRx<T>.Create(MBButtonType.ICON, ConfirmExecutionAsync, Color, null, _hasProgress, null, _cancelColor);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(_buttonRx);
            ArgumentNullException.ThrowIfNull(Icon);
            _buttonRx.SetParameter(State, _changeStateAsync, _changeStateAsyncCancel, CanChange, _deferredNotification);

            Icon = _buttonRx.GetIconButtonParameters(State, Icon, IconVariant);
            Color = _buttonRx.Color;
            OnClick = (EventCallback<MouseEventArgs>)_buttonRx.OnClick;
            Disabled = _buttonRx.Disabled;

            base.OnParametersSet();
        }
    }
}
