
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

            Services.AddSingleton(sp => new TestService(sp));
            Services.AddSingleton<TimerService>();

            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
