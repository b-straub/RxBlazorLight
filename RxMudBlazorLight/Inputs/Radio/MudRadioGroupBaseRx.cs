using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.Inputs.Radio
{
    public class MudRadioGroupBaseRx<T> : MudRadioGroup<T>
    {
        [Parameter]
        public Func<T, bool> DenseCallback { get; set; } = _ => false;

        [Parameter]
        public Func<T, Size> SizeCallback { get; set; } = _ => Size.Medium;

        [Parameter]
        public Func<T, Color> ColorCallback { get; set; } = _ => Color.Default;

        [Parameter]
        public Func<T, Placement> PlacementCallback { get; set; } = _ => Placement.End;

        protected IInputGroup<T>? RxInputGroupBase { get; set; }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupBase);

            if (ChildContent is null)
            {
                var values = RxInputGroupBase.GetItems();

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
                        builder.AddAttribute(7, "Disabled", RxInputGroupBase.IsItemDisabled(i));
                        builder.AddAttribute(8, "InitialSelection", RxInputGroupBase.Value);
                        builder.CloseComponent();
                    }
                };
            }

            if (RxInputGroupBase.State is not InputState.CHANGING)
            {
                Value = RxInputGroupBase.Value;
                ValueChanged = EventCallback.Factory.Create<T>(this, v => RxInputGroupBase.Value = v);
            }
            Disabled = !RxInputGroupBase.CanChange() || RxInputGroupBase.State is InputState.CHANGING;

            base.OnParametersSet();
        }
    }
}
