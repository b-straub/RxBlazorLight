using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.Inputs.Select
{
    public class MudSelectAsyncBaseRx<T> : MudSelect<T>
    {
        [Parameter]
        public bool HideDisabled { get; set; } = false;

        protected IInputGroupAsync<T>? RxInputGroupAsyncBase { get; set; }

        private bool _initialized = false;
        private bool _initializedAsync = false;

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupAsyncBase);

            Value = RxInputGroupAsyncBase.Value;
            Text = Value?.ToString();

            ValueChanged = EventCallback.Factory.Create<T>(this, async v => await RxInputGroupAsyncBase.SetValueAsync(v));
            Disabled = !RxInputGroupAsyncBase.CanChange();

            if (ChildContent is null)
            {
                var values = RxInputGroupAsyncBase.GetItems();

                ChildContent = builder =>
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (HideDisabled && RxInputGroupAsyncBase.IsItemDisabled(i))
                        {
                            continue;
                        }

                        builder.OpenComponent(0, typeof(MudSelectItemRx<T>));
                        builder.AddAttribute(1, "Index", i);
                        builder.AddAttribute(2, "Values", values);
                        builder.AddAttribute(3, "Disabled", RxInputGroupAsyncBase.IsItemDisabled(i));
                        builder.CloseComponent();
                    }
                };
            }

            base.OnParametersSet();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupAsyncBase);

            if (!_initialized && firstRender)
            {
                _initialized = true;
                RxInputGroupAsyncBase.Initialize();
                Value = RxInputGroupAsyncBase.Value;
                Text = Value?.ToString();
            }

            base.OnAfterRender(firstRender);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupAsyncBase);

            if (!_initializedAsync && firstRender)
            {
                _initializedAsync = true;
                await RxInputGroupAsyncBase.InitializeAsync();
                Value = RxInputGroupAsyncBase.Value;
                Text = Value?.ToString();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
