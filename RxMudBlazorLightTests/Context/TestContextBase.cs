
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

            Services.AddRxBLService(sp => new TestService(sp));
            Services.AddRxBLService<TimerService>();

            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
