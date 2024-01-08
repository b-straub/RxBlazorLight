
using MudBlazor.Services;
using RxMudBlazorLightTestBase.Service;

namespace RxMudBlazorLightTests.Context
{
    public class TestContextBase : TestContext
    {
        public TestContextBase() 
        {
            Services.AddMudServices();
            Services.AddSingleton<TestService>();
            Services.AddScoped<TimerService>();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
