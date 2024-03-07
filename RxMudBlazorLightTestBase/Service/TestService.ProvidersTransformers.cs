using RxBlazorLightCore;
using static MudBlazor.Colors;
using System.Threading;
using static RxMudBlazorLightTestBase.Service.TimerService;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class EqualsTestSyncSP(TestService service) : StateProvider<TestService>(service)
        {
            protected override bool CanProvide()
            {
                return Service._equalTestValue < 1;
            }

            protected override void ProvideState()
            {
                Service._equalTestValue++;
            }
        }

        public class EqualsTestAsyncSP(TestService service) : StateProviderAsync<TestService>(service)
        {
            protected override bool CanProvide()
            {
                return Service._equalTestAsyncValue < 2;
            }

            protected override async Task ProvideStateAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(500, cancellationToken);
                Service._equalTestAsyncValue += 10;
            }
        }

        public class ExceptionSP(TestService service, IState<int> state) : StateProvider<TestService, int>(service, state)
        {
            protected override int ProvideValue()
            {
                throw new InvalidOperationException("Command test exception!");
            }
        }

        public class IncrementSP(TestService service, IState<int> state) : StateProvider<TestService, int>(service, state)
        {
            public override bool CanProvide(int _)
            {
                return Service._canIncrement;
            }

            protected override int ProvideValue()
            {
                return State.Value + 1;
            }
        }

        public class AddST(TestService service, IState<int> state) : StateTransformer<TestService, int, int>(service, state)
        {
            public override bool CanTransform(int _)
            {
                return State.Value > 1;
            }
            protected override int TransformState(int value)
            {
                return State.Value + value;
            }
        }

        public class IncrementAsyncSP(TestService service, IState<int> state) : StateProviderAsync<TestService, int>(service, state)
        {
            protected override async Task<int> ProvideValueAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(500, cancellationToken);
                return State.Value + 1;
            }

            public override bool CanProvide(int v)
            {
                return Service.CountState.Value > v;
            }
        }

        public class AddAsyncST(TestService service, IState<int> state) : StateTransformerAsync<TestService, int, int>(service, state)
        {
            public override bool CanTransform(int _)
            {
                return State.Value > 2;
            }

            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(4000, cancellationToken);
                return State.Value + value;
            }
            public override bool CanCancel => true;
            public override bool LongRunning => true;
        }

        public class AddRemoveAsyncST(TestService service, IState<int> state) : StateTransformerAsync<TestService, int, int>(service, state)
        {
            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(5000, cancellationToken);

                int val = Service.AddMode.Value ? value : -value;
                return Service.CountState.Value + val;
            }

            public override bool CanTransform(int _)
            {
                return !Service.AddMode.Value || Service.CountState.Value < 30;
            }

            public override bool CanCancel => true;
            public override bool LongRunning => true;
        }

        public class PizzaChangeAsyncST(TestService service, IState<Pizza> state) : StateTransformerAsync<TestService, Pizza, Pizza>(service, state)
        {
            public override bool CanTransform(Pizza? pizza)
            {
                return pizza?.Name != "Diavolo";
            }

            protected override async Task<Pizza> TransformStateAsync(Pizza value, CancellationToken cancellationToken)
            {
                await Task.Delay(1000, cancellationToken);
                return value;
            }
        }
    }
}
