using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class MudFabAsyncBaseRx : MudFab
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
            _buttonRx = ButtonRx.Create(MBButtonType.FAB, ConfirmExecutionAsync, Color, null, _hasProgress, _cancelText, _cancelColor);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(_buttonRx);
            _buttonRx.SetParameter(StateCommand, _changeStateAsync, _changeStateAsyncCancel, CanChange, _deferredNotification);
                            
            var parameters = _buttonRx.GetFabParameters(StateCommand, StartIcon, EndIcon, Label, IconVariant, _changeStateAsyncCancel is not null);

            StartIcon = parameters.StartIcon;
            EndIcon = parameters.EndIcon;
            Label = parameters.Label;

            Color = _buttonRx.Color;
            OnClick = (EventCallback<MouseEventArgs>)_buttonRx.OnClick;

            Disabled = _buttonRx.Disabled;

            base.OnParametersSet();
        }
    }
}
