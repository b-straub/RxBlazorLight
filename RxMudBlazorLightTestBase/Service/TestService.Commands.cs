using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service
{
    public sealed partial class TestService
    {
        public class SimpleCMD : Command
        {
            protected override void DoExecute()
            {
            }
        }

        public class EqualsTestCmd : Command<int>
        {
            private int? _value;
            protected override void DoExecute(int parameter)
            {
                _value = parameter;
            }

            public override bool CanExecute(int parameter)
            {
                return !parameter.Equals(_value);
            }
        }

        public class EqualsTestAsyncCmd : CommandAsync<int>
        {
            private int? _value;

            protected override Task DoExecute(int parameter, CancellationToken cancellationToken)
            {
                _value = parameter;
                return Task.CompletedTask;
            }

            public override bool CanExecute(int parameter)
            {
                return !parameter.Equals(_value);
            }
        }

        public class ExceptionCMD : CommandService<TestService>
		{
            public ExceptionCMD(TestService testService) : base(testService)
            {
            }

            protected override void DoExecute()
			{
                throw new InvalidOperationException("Command test exception!");
			}
		}

		public class IncrementCMD : CommandService<TestService>
        {
            public IncrementCMD(TestService testService) : base(testService) 
            { 
            }

            protected override void DoExecute()
            {
                Service.Count++;
            }

            public override bool CanExecute()
            {
                return Service._canIncrement;
            }
        }

        public class AddIncrementValueCMD : CommandService<TestService>
        {
            public AddIncrementValueCMD(TestService testService) : base(testService) { }

            protected override void DoExecute()
            {
                Service.Count += Service.IncrementValue.Value;
            }

            public override bool CanExecute()
            {
                return Service.IncrementValue.Value > 5;
            }
        }

        public class AddCMD : CommandService<TestService, int>
        {
            public AddCMD(TestService testService) : base(testService) { }

            protected override void DoExecute(int parameter)
            {
                Service.Count += parameter;
            }

            public override bool CanExecute(int parameter)
            {
                return parameter < 10 && Service.Count > 1;
            }
        }

        public class IncrementAsyncCMD : CommandServiceAsync<TestService>
        {
            public IncrementAsyncCMD(TestService testService) : base(testService) { }

            protected override async Task DoExecute(CancellationToken _)
            {
                await Task.Delay(500, CancellationToken.None);
                Service.Count++;
            }

            public override bool CanExecute()
            {
                return Service._canIncrement;
            }
        }

        public class AddAsyncCMD : CommandLongRunningServiceAsync<TestService, int>
        {
            public AddAsyncCMD(TestService testService) : base(testService) { }

            protected override async Task DoExecute(int parameter, CancellationToken ct)
            {
                await Task.Delay(4000, ct);
                Service.Count += parameter;
            }

            public override bool CanExecute(int parameter)
            {
                return Service.Count > 2;
            }
        }

        public class AddAsyncCMDForm : CommandLongRunningServiceAsync<TestService, int>
        {
            public AddAsyncCMDForm(TestService testService) : base(testService) { }

            protected override async Task DoExecute(int parameter, CancellationToken ct)
            {
                await Task.Delay(100, ct);
                Service.Count += (parameter / 3);
            }

            public override bool CanExecute(int parameter)
            {
                return Service.CanIncrementCheck is not null && Service.CanIncrementCheck.Value;
            }
        }

        public class AddRemoveAsyncCMDForm : CommandLongRunningServiceAsync<TestService, int>
        {
            public AddRemoveAsyncCMDForm(TestService testService) : base(testService) { }

            protected override async Task DoExecute(int parameter, CancellationToken ct)
            {
                await Task.Delay(5000, ct);

                int val = Service.AddMode.Value ? (int)parameter : -(int)parameter;
                Service.Count += val;
            }

            public override bool CanExecute(int parameter)
            {
                return !Service.AddMode.Value || Service.Count < 30;
            }
        }
    }
}
