﻿using RxBlazorLightCore;
using static RxMudBlazorLightTestBase.Service.TimerService;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class EqualsTestVP(TestService service) : ServiceStateProvider<TestService>(service)
        {
            public override bool CanRun => Service._equalTestValue < 1;

            protected override void ProvideState()
            {
                Service._equalTestValue++;
            }
        }

        public class EqualsTestSPAsync(TestService service) : ServiceStateTransformerAsync<TestService, int>(service)
        {
            public override bool CanRun => Service._equalTestAsyncValue < 2;

            protected override async Task TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(500, cancellationToken);
                Service._equalTestAsyncValue += value;
            }
        }

        public class ExceptionVP(TestService service, IState<int> state) : StateProvider<TestService, int>(service, state)
        {
            protected override int ProvideValue()
            {
                throw new InvalidOperationException("Command test exception!");
            }
        }

        public class IncrementVP(TestService service, IState<int> state) : StateProvider<TestService, int>(service, state)
        {
            public override bool CanRun => Service._canIncrement;
     
            protected override int ProvideValue()
            {
                return State.Value + 1;
            }
        }

        public class AddSP(TestService service, IState<int> state) : StateTransformer<TestService, int, int>(service, state)
        {
            public override bool CanRun => State.Value > 1;

            protected override int TransformState(int value)
            {
                return State.Value + value;
            }
        }

        public class IncrementVPAsync(TestService service, IState<int> state) : StateProviderAsync<TestService, int>(service, state)
        {
            public override bool CanRun => Service._canIncrement;

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

        public class AddSPAsync(TestService service, IState<int> state) : StateTransformerAsync<TestService, int, int>(service, state)
        {
            public override bool CanRun => State.Value > 2;

            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(4000, cancellationToken);
                return State.Value + value;
            }
            public override bool CanCancel => true;
            public override bool LongRunning => true;
        }

        public class AddRemoveAsyncSP(TestService service, IState<int> state) : StateTransformerAsync<TestService, int, int>(service, state)
        {
            protected override async Task<int> TransformStateAsync(int value, CancellationToken cancellationToken)
            {
                await Task.Delay(5000, cancellationToken);

                int val = Service.AddMode.Value ? value : -value;
                return Service.CountState.Value + val;
            }

            public override bool CanRun => !Service.AddMode.Value || Service.CountState.Value < 30;

            public override bool CanCancel => true;
            public override bool LongRunning => true;
        }
    }
}
