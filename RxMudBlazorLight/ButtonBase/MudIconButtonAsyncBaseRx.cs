using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class MudIconButtonAsyncBaseRx : MudIconButton
    {
        [Parameter, EditorRequired]
        public required IStateCommandAsync StateCommand { get; init; }

        [Parameter]
        public Func<bool>? CanChange { get; init; }

        [Parameter]
        public Func<Task<bool>>? ConfirmExecutionAsync { get; init; }

        [Parameter]
        public MBIconVariant? IconVariant { get; set; }

        protected Func<Task>? _changeStateAsync;
        protected Func<CancellationToken, Task>? _changeStateAsyncCancel;
        protected string? _cancelText;
        protected Color? _cancelColor;
        protected bool _hasProgress = false;
        protected bool _deferredNotification = false;
        protected RenderFragment RenderBase() => base.BuildRenderTree;

        internal ButtonRx? _buttonRx;

        protected override void OnInitialized()
        {
            _buttonRx = ButtonRx.Create(MBButtonType.ICON, ConfirmExecutionAsync, Color, null, _hasProgress, null, _cancelColor);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(_buttonRx);
            ArgumentNullException.ThrowIfNull(Icon);
            _buttonRx.SetParameter(StateCommand, _changeStateAsync, _changeStateAsyncCancel, CanChange, _deferredNotification);

            Icon = _buttonRx.GetIconButtonParameters(StateCommand, Icon, IconVariant);
            Color = _buttonRx.Color;
            OnClick = (EventCallback<MouseEventArgs>)_buttonRx.OnClick;
            Disabled = _buttonRx.Disabled;

            base.OnParametersSet();
        }
    }
}
