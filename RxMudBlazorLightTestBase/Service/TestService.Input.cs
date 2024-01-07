using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class IncrementValueIP(TestService service, int value) : Input<TestService, int>(service, value)
        {
            protected override void OnValueChanging(int oldValue, int newValue)
            {
                Service.Count = newValue;
            }

            public override bool CanChange()
            {
                return Service.CanIncrementCheck.Value;
            }
        }

        public class AddModeIP(TestService service, bool value) : Input<TestService, bool>(service, value)
        {
            public override bool CanChange()
            {
                return !Service.AddRemoveAsync.Running();
            }
        }

        public class RatingValueIP(TestService service, int value) : Input<TestService, int>(service, value)
        {
            public override bool CanChange()
            {
                return Service.GetRadio().Value?.Color is ColorEnum.GREEN;
            }
        }

        public class PizzaIPGAsync(TestService service) : InputGroupAsync<TestService, Pizza?>(service, null)
        {
            private static readonly Pizza[] _pizzas =
            [
                new("Cardinale"), new("Diavolo"), new("Margarita"), new("Spinaci")
            ];

            private bool _initialized = false;

            public override Pizza[] GetItems()
            {
                
                return _pizzas;
            }

            public override bool CanChange()
            {
                return _initialized;
            }

            public override void Initialize()
            {
                SetInitialValue(_pizzas[3]);
            }

            public override async Task InitializeAsync()
            {
                await Task.Delay(2000);
                await SetInitialValueAsync(_pizzas[1]);
                _initialized = true;
            }
        }

        public class PizzaIPG(TestService service, Pizza value) : InputGroup<TestService, Pizza>(service, value)
        {
            public static readonly Pizza[] Pizzas =
            [
                new("Cardinale"), new("Diavolo"), new("Margarita"), new("Spinaci")
            ];

            public override Pizza[] GetItems()
            {

                return Pizzas;
            }
        }

        public class ColorIPGP(TestService service) : InputGroupP<TestService, TestColor, ColorEnum>(service, _colors[0])
        {
            private static readonly TestColor[] _colors =
            [
                new(ColorEnum.RED), new(ColorEnum.GREEN), new(ColorEnum.BLUE)
            ];

            private bool _initialized = false;

            public override TestColor[] GetItems()
            {
                return _colors;
            }

            public override void Initialize()
            {
                if (!_initialized)
                {
                    _initialized = true;
                    SetInitialValue(new TestColor(Parameter));
                }
            }

            public override bool IsItemDisabled(int index)
            {
                return index == 1 && Service.CanIncrementCheck.Value;
            }
        }
    }
}
