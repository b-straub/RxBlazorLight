
using MudBlazor.Services;
using RxBlazorLightCore;
using RxMudBlazorLightTestBase.Service;

namespace RxMudBlazorLightTests.Context
{
    public class TestContextBase : TestContext
    {
        public TestContextBase()
        {
            Services.AddMudServices();

            var collector = Services.AddRxBLServiceCollector();
            Services.AddRxBLService(collector, sp => new TestService(sp));
            Services.AddRxBLService<TimerService>(collector);

            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
