
using MudBlazor.Services;
using RxMudBlazorLightTestBase.Service;
using RxBlazorLightCore;

namespace RxMudBlazorLightTests.Context
{
    public class TestContextBase : TestContext
    {
        public TestContextBase() 
        {
            Services.AddMudServices();
            Services.AddRxBLService(sp => new TestService(sp));
            Services.AddSingleton<TimerService>();
            Services.AddRxBLService<TimerService>();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
