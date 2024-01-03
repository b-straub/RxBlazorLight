
namespace RxMudBlazorLight.ButtonBase
{
    public enum MBButtonType
    {
        DEFAULT,
        FAB,
        ICON,
        TOGGLEICON
    }

    public class ButtonBaseRX
    {
        protected readonly MBButtonType _type;

        protected readonly Action? _beforeExecution;

        protected readonly Action? _afterExecution;

        protected ButtonBaseRX(MBButtonType Type, Action? beforeExecution, Action? afterExecution)
        {
            _type = Type;
            _beforeExecution = beforeExecution;
            _afterExecution = afterExecution;
        }
    }
}