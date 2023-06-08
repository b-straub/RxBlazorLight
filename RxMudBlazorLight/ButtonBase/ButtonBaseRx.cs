
namespace RxBlazorLight.ButtonBase
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

        protected readonly Func<Task<bool>>? _confirmExecution;

        protected readonly Action? _beforeExecution;

        protected readonly Action? _afterExecution;

        protected ButtonBaseRX(MBButtonType Type, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            _type = Type;
            _confirmExecution = confirmExecution;
            _beforeExecution = beforeExecution;
            _afterExecution = afterExecution;
        }
    }
}