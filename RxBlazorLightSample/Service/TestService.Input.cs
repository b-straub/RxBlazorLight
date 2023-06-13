using RxBlazorLightCore;

namespace RxBlazorLightSample.Service
{
    public sealed partial class TestService
    {
        public class IncrementValueIP : Input<TestService, int>
        {
            public IncrementValueIP(TestService service, int value) : base(service, value)
            {
            }

            protected override void OnValueChanged(int oldValue, int newValue)
            {
                Service.Count = newValue;
            }

            public override bool CanChange()
            {
                return Service.CanIncrementCheck.Value;
            }
        }

        public class AddModeIP : Input<TestService, bool>
        {
            public AddModeIP(TestService service, bool value) : base(service, value)
            {
            }

            public override bool CanChange()
            {
                return !Service.AddRemoveAsync.Executing;
            }
        }

        public class RatingValueIP : Input<TestService, int>
        {
            public RatingValueIP(TestService service, int value) : base(service, value)
            {
            }

            public override bool CanChange()
            {
                return Service.RadioTest.Value?.Color is ColorEnum.GREEN;
            }
        }

        public class PizzaIPGAsync : InputGroupAsync<TestService, Pizza?>
        {
            private static readonly Pizza[] _pizzas =
            {
                new Pizza("Cardinale"), new Pizza("Diavolo"), new Pizza("Margarita"), new Pizza("Spinaci")
            };

            private bool _initialized = false;

            public PizzaIPGAsync(TestService service) : base(service, null)
            {

            }

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
                SetValue(_pizzas[2]);
            }

            public override async Task InitializeAsync()
            {
                await Task.Delay(2000);
                await SetValueAsync(_pizzas[1]);
                _initialized = true;
            }
        }

        public class PizzaIPG : InputGroup<TestService, Pizza>
        {
            public static readonly Pizza[] Pizzas =
            {
                new Pizza("Cardinale"), new Pizza("Diavolo"), new Pizza("Margarita"), new Pizza("Spinaci")
            };

            public PizzaIPG(TestService service, Pizza value) : base(service, value)
            {

            }

            public override Pizza[] GetItems()
            {

                return Pizzas;
            }
        }

        public class ColorIPG : InputGroup<TestService, TestColor>
        {
            private static readonly TestColor[] _colors =
            {
                new TestColor(ColorEnum.RED), new TestColor(ColorEnum.GREEN), new TestColor(ColorEnum.BLUE)
            };

            public ColorIPG(TestService service) : base(service, _colors[0])
            {

            }

            public override TestColor[] GetItems()
            {

                return _colors;
            }

            public override void Initialize()
            {
                SetValue(_colors[2]);
            }

            public override bool IsItemDisabled(int index)
            {
                return index == 1 && Service.CanIncrementCheck.Value;
            }
        }

        public class ColorIPGP : InputGroupP<TestService, TestColor, ColorEnum>
        {
            private static readonly TestColor[] _colors =
            {
                new TestColor(ColorEnum.RED), new TestColor(ColorEnum.GREEN), new TestColor(ColorEnum.BLUE)
            };

            public ColorIPGP(TestService service) : base(service, _colors[0])
            {
            }

            public override TestColor[] GetItems()
            {
                return _colors;
            }

            public override void Initialize()
            {
                SetValue(new TestColor(Parameter));
            }

            public override bool IsItemDisabled(int index)
            {
                return index == 1 && Service.CanIncrementCheck.Value;
            }
        }
    }
}
