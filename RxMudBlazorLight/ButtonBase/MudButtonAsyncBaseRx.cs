using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.ButtonBase
{
    public class MudButtonAsyncBaseRx<T> : MudButton
    {
        [Parameter, EditorRequired]
        public required IStateAsync<T> State { get; init; }

        [Parameter]
        public Func<IStateAsync<T>, bool>? CanChange { get; init; }

        [Parameter]
        public Func<Task<bool>>? ConfirmExecutionAsync { get; init; }

        [Parameter]
        public bool DeferredNotification { get; set; }

        protected Func<IStateAsync<T>, Task>? _changeStateAsync;
        protected Func<IStateAsync<T>, CancellationToken, Task>? _changeStateAsyncCancel;
        protected string? _cancelText;
        protected Color? _cancelColor;
        protected bool _hasProgress = false;

        protected RenderFragment RenderBase() => base.BuildRenderTree;

        private ButtonRx<T>? _buttonRx;

        protected override void OnInitialized()
        {
            _buttonRx = ButtonRx<T>.Create(MBButtonType.DEFAULT, ConfirmExecutionAsync, Color, ChildContent, _hasProgress, _cancelText, _cancelColor);

            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(_buttonRx);
            _buttonRx.SetParameter(State, _changeStateAsync, _changeStateAsyncCancel, CanChange, DeferredNotification);

            ChildContent = _buttonRx.ChildContent;
            Color = _buttonRx.Color;
            OnClick = (EventCallback<MouseEventArgs>)_buttonRx.OnClick;
            Disabled = _buttonRx.Disabled;

            base.OnParametersSet();
        }
    }
}
