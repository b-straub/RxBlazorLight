using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;

namespace RxMudBlazorLight.Inputs.Select
{
    public class MudSelectBaseRx<T> : MudSelect<T>
    {
        [Parameter]
        public bool HideDisabled { get; set; } = false;

        protected IInputGroup<T>? RxInputGroupBase { get; set; }

        private bool _initialized = false;

        protected override void OnParametersSet()
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupBase);

            if (RxInputGroupBase.HasValue())
            {
                Value = RxInputGroupBase.Value;
                Text = Value.ToString();
            }

            ValueChanged = EventCallback.Factory.Create<T>(this, v => RxInputGroupBase.Value = v);
            Disabled = !RxInputGroupBase.CanChange();

            if (ChildContent is null)
            {
                var values = RxInputGroupBase.GetItems();

                ChildContent = builder =>
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (HideDisabled && RxInputGroupBase.IsItemDisabled(i))
                        {
                            continue;
                        }

                        builder.OpenComponent(0, typeof(MudSelectItemRx<T>));
                        builder.AddAttribute(1, "Index", i);
                        builder.AddAttribute(2, "Values", values);
                        builder.AddAttribute(3, "Disabled", RxInputGroupBase.IsItemDisabled(i));
                        builder.CloseComponent();
                    }
                };
            }

            Disabled = !RxInputGroupBase.CanChange() || RxInputGroupBase.State is InputState.CHANGING;

            base.OnParametersSet();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            ArgumentNullException.ThrowIfNull(RxInputGroupBase);

            if (!_initialized && firstRender && RxInputGroupBase.HasValue()) 
            {
                _initialized = true;
                Value = RxInputGroupBase.Value;
                Text = Value.ToString();
            }

            base.OnAfterRender(firstRender);
        }
    }
}
