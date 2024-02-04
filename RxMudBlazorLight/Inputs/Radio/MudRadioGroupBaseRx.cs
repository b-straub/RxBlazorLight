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

        protected IStateGroup<T>? RxStateGroupBase { get; set; }

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxStateGroupBase);

            if (ChildContent is null)
            {
                var values = RxStateGroupBase.GetItems();

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
                        builder.AddAttribute(7, "Disabled", RxStateGroupBase.IsItemDisabled(i));
                        builder.AddAttribute(8, "InitialSelection", RxStateGroupBase.Value);
                        builder.CloseComponent();
                    }
                };
            }

            if (RxStateGroupBase.Phase is not StateChangePhase.CHANGING)
            {
                Value = RxStateGroupBase.Value;
                ValueChanged = EventCallback.Factory.Create<T>(this, RxStateGroupBase.Transform);
            }
            Disabled = !RxStateGroupBase.CanRun || RxStateGroupBase.Phase is StateChangePhase.CHANGING;

            base.OnParametersSet();
        }
    }
}
