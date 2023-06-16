
using MudBlazor.Services;
using RxMudBlazorLightTestBase.Service;

namespace RxMudBlazorLightTests.Context
{
    public class TestContextBase : TestContext
    {
        public TestContextBase() 
        {
            Services.AddMudServices();
            Services.AddScoped<TestService>();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
