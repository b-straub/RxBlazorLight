using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.Inputs.Radio
{
    public class MudRadioGroupAsyncBaseRx<T> : MudRadioGroup<T>
    {
        [Parameter]
        public Func<T, bool> DenseCallback { get; set; } = _ => false;

        [Parameter]
        public Func<T, Size> SizeCallback { get; set; } = _ => Size.Medium;

        [Parameter]
        public Func<T, Color> ColorCallback { get; set; } = _ => Color.Default;

        [Parameter]
        public Func<T, Placement> PlacementCallback { get; set; } = _ => Placement.End;

        protected IInputGroupAsync<T>? RxInputGroupAsyncBase { get; set; }

        private bool _initializedAsync = false;

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupAsyncBase);

            if (ChildContent is null)
            {
                RxInputGroupAsyncBase.Initialize();

                var values = RxInputGroupAsyncBase.GetItems();

                ChildContent = builder =>
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        builder.OpenComponent(0, typeof(MudRadioRx<T>));
                        builder.AddAttribute(1, "Index", i);
                        builder.AddAttribute(2, "Values", values);
                        builder.AddAttribute(3, "Dense", DenseCallback(values[i]));
                        builder.AddAttribute(4, "Size", SizeCallback(values[i]));
                        builder.AddAttribute(5, "Color", ColorCallback(values[i]));
                        builder.AddAttribute(6, "Placement", PlacementCallback(values[i]));
                        builder.AddAttribute(7, "Disabled", RxInputGroupAsyncBase.IsItemDisabled(i));
                        builder.AddAttribute(8, "InitialSelection", RxInputGroupAsyncBase.Value);
                        builder.CloseComponent();
                    }
                };
            }

            Value = RxInputGroupAsyncBase.Value;
            ValueChanged = EventCallback.Factory.Create<T>(this, v => RxInputGroupAsyncBase.SetValueAsync(v));
            Disabled = !RxInputGroupAsyncBase.CanChange();

            base.OnParametersSet();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupAsyncBase);

            if (!_initializedAsync && firstRender)
            {
                _initializedAsync = true;
                await RxInputGroupAsyncBase.InitializeAsync();
                Value = RxInputGroupAsyncBase.Value;
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
