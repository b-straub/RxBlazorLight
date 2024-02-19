using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.Inputs.Select
{
    public class MudSelectBaseRx<T> : MudSelect<T>
    {
        [Parameter]
        public bool HideDisabled { get; set; } = false;

        protected IStateGroup<T>? RxStateGroupBase { get; set; }

        private bool _initialized = false;

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxStateGroupBase);

            if (RxStateGroupBase.HasValue())
            {
                Value = RxStateGroupBase.Value;
                Text = Value.ToString();
            }

            ValueChanged = EventCallback.Factory.Create<T>(this, RxStateGroupBase.Transform);
            Disabled = !RxStateGroupBase.CanTransform(RxStateGroupBase.Value);

            if (ChildContent is null)
            {
                var values = RxStateGroupBase.GetItems();

                ChildContent = builder =>
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (HideDisabled && RxStateGroupBase.IsItemDisabled(i))
                        {
                            continue;
                        }

                        builder.OpenComponent(0, typeof(MudSelectItemRx<T>));
                        builder.AddAttribute(1, "Index", i);
                        builder.AddAttribute(2, "Values", values);
                        builder.AddAttribute(3, "Disabled", RxStateGroupBase.IsItemDisabled(i));
                        builder.CloseComponent();
                    }
                };
            }

            Disabled = !RxStateGroupBase.CanTransform(RxStateGroupBase.Value) || RxStateGroupBase.Phase is StateChangePhase.CHANGING;

            base.OnParametersSet();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            ArgumentNullException.ThrowIfNull(RxStateGroupBase);

            if (!_initialized && firstRender && RxStateGroupBase.HasValue()) 
            {
                _initialized = true;
                Value = RxStateGroupBase.Value;
                Text = Value.ToString();
            }

            base.OnAfterRender(firstRender);
        }
    }
}
