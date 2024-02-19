using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class IncrementStateVP(TestService service, IState<int> state) : StateTransformerAsync<TestService, int, int>(service, state)
        {
            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(3000, cancellationToken);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 40);
                return value;
            }

            public override bool CanTransform(int _)
            {
                return Service.CanIncrementCheck.Value;
            }
            public override bool CanCancel => true;
            public override bool LongRunning => true;
        }

        public class IncrementStateAddVP(TestService service, IState<int> state) : StateTransformer<TestService, int, int>(service, state)
        {
            public override bool CanTransform(int value)
            {
                return State.Value > value;
            }
            protected override int TransformState(int value)
            {
                return State.Value + value;
            }
        }

        public class AddModeVP(TestService service, IState<bool> state) : StateTransformerDirect<TestService, bool>(service, state)
        {
            public override bool CanTransform(bool _)
            {
                return !Service.AddAsync.Changing();
            }
        }

        public class RatingValueVP(TestService service, IState<int> state) : StateTransformerDirect<TestService, int>(service, state)
        {
            public override bool CanTransform(int _)
            {
                return Service.GetRadio().Value?.Color is ColorEnum.GREEN;
            }
        }

        public class PizzaSG(TestService service, Pizza value) : StateGroup<TestService, Pizza>(service, value)
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

        public class ColorSG(TestService service) : StateGroup<TestService, TestColor>(service, _colors[0])
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

            public override bool IsItemDisabled(int index)
            {
                return index == 1 && Service.CanIncrementCheck.Value;
            }
        }
    }
}
