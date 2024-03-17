using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class MudFabAsyncBaseRx<T> : MudFab
    {
        [Parameter, EditorRequired]
        public required IStateAsync<T> State { get; init; }

        [Parameter]
        public Func<IStateAsync<T>, bool>? CanChange { get; init; }

        [Parameter]
        public Func<Task<bool>>? ConfirmExecutionAsync { get; init; }

        [Parameter]
        public bool DeferredNotification { get; set; } = false;

        [Parameter]
        public MBIconVariant? IconVariant { get; set; }

        protected Func<IStateAsync<T>, Task>? _changeStateAsync;
        protected Func<IStateAsync<T>, CancellationToken, Task>? _changeStateAsyncCancel;
        protected string? _cancelText;
        protected Color? _cancelColor;
        protected bool _hasProgress = false;

        protected RenderFragment RenderBase() => base.BuildRenderTree;
        internal ButtonRx<T>? _buttonRx;

        protected override void OnInitialized()
        {
            _buttonRx = ButtonRx<T>.Create(MBButtonType.FAB, ConfirmExecutionAsync, Color, null, _hasProgress, _cancelText, _cancelColor);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(_buttonRx);
            _buttonRx.SetParameter(State, _changeStateAsync, _changeStateAsyncCancel, CanChange, DeferredNotification);

            var parameters = _buttonRx.GetFabParameters(State, StartIcon, EndIcon, Label, IconVariant, _changeStateAsyncCancel is not null);

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
