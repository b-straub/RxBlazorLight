using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class IncrementValueIP(TestService service, int value) : Input<TestService, int>(service, value)
        {
            protected override async ValueTask OnValueChangedAsync(int oldValue, int newValue, CancellationToken cancellationToken)
            {
                await Task.Delay(3000, cancellationToken);

                ArgumentOutOfRangeException.ThrowIfGreaterThan(newValue, 40);

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

        public class PizzaIPG(TestService service, Pizza value) : InputGroup<TestService, Pizza>(service, value)
        {
            public static readonly Pizza[] Pizzas =
            [
                new("Cardinale"),
                new("Diavolo"),
                new("Margarita"),
                new("Spinaci")
            ];

            public override Pizza[] GetItems()
            {

                return Pizzas;
            }
        }

        public class ColorIPGP(TestService service) : InputGroup<TestService, TestColor>(service, _colors[0])
        {
            private static readonly TestColor[] _colors =
            [
                new(ColorEnum.RED),
                new(ColorEnum.GREEN),
                new(ColorEnum.BLUE)
            ];

            public override TestColor[] GetItems()
            {
                return _colors;
            }

            protected override async ValueTask OnValueChangedAsync(TestColor? oldValue, TestColor newValue, CancellationToken cancellationToken)
            {
                await Task.Delay(2000, cancellationToken);
            }

            public override bool IsItemDisabled(int index)
            {
                return index == 1 && Service.CanIncrementCheck.Value;
            }
        }
    }
}
