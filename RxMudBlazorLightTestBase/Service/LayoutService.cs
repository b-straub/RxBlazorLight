using RxBlazorLightCore;

namespace RxMudBlazorLightTestBase.Service;

public class LayoutService : RxBLService
{
    public IState<bool> DarkMode { get; } 

    public LayoutService() 
    {
        DarkMode = this.CreateState(false);
    }
    
    protected override async ValueTask ContextReadyAsync()
    {
        Console.WriteLine("LayoutService OnContextInitialized");
        await Task.Delay(3000);
    }
}