using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RxBlazorLightCore;
using System.Diagnostics.CodeAnalysis;

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

    public class ButtonRX : ButtonBaseRX
    {
        public EventCallback<MouseEventArgs> OnClick { get; }

        private ButtonRX(MBButtonType type, ICommand? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution) :
            base(type, confirmExecution, beforeExecution, afterExecution)
        {
            ArgumentNullException.ThrowIfNull(command);
            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, async () => await Execute(command.Execute));
        }

        public static ButtonRX Create(MBButtonType type, ICommand? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            return new ButtonRX(type, command, confirmExecution, beforeExecution, afterExecution);
        }

        private async Task Execute(Action execute)
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }

    public class ButtonPRX<T> : ButtonBaseRX
    {
        public EventCallback<MouseEventArgs> OnClick { get; private set; }

        private readonly ICommand<T> _command;

        private ButtonPRX(MBButtonType type, ICommand<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution) :
            base(type, confirmExecution, beforeExecution, afterExecution)
        {
            ArgumentNullException.ThrowIfNull(command);
            _command = command;
        }

        public static ButtonPRX<T> Create(MBButtonType type, ICommand<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution)
        {
            return new ButtonPRX<T>(type, command, confirmExecution, beforeExecution, afterExecution);
        }

        public void SetParameters(T? parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);

            _command.SetParameter(parameter);
            OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, async () => await Execute(_command.Execute));
        }

        private async Task Execute(Action execute)
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }

    public enum MBIconVariant
    {
        Filled,
        Outlined,
        Rounded,
        Sharp,
        TwoTone
    }

    public partial class ButtonBaseAsyncRX : ButtonBaseRX
    {
        public bool Disabled { get; protected set; } = true;
        public Color Color { get; protected set; }
        public RenderFragment? ChildContent { get; protected set; }

        protected readonly Color _buttonColor;
        protected readonly RenderFragment? _buttonChildContent;

        private readonly string? _cancelText;

        private enum IconForState
        {
            None,
            Start,
            End
        }

        private IconForState _iconForState = IconForState.None;
        private string? _buttonLabel;
        private string? _buttonIcon;

        protected ButtonBaseAsyncRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, string? cancelText) :
           base(type, confirmExecution, beforeExecution, afterExecution)
        {
            _buttonColor = buttonColor;
            Color = _buttonColor;

            _buttonChildContent = buttonChildContent;
            ChildContent = _buttonChildContent;
            _cancelText = cancelText;

            if (type is MBButtonType.DEFAULT)
            {
                _cancelText = cancelText ?? "Cancel";
            }
        }

        public RenderFragment RenderCancel() => builder =>
        {
            switch (_type)
            {
                case MBButtonType.DEFAULT:
                    builder.OpenComponent<MudProgressCircular>(0);
                    builder.AddAttribute(1, "Indeterminate", true);
                    builder.AddAttribute(2, "Size", Size.Small);
                    builder.AddAttribute(3, "Class", "ms-n1");
                    builder.CloseComponent();
                    builder.OpenComponent<MudText>(4);
                    builder.AddAttribute(5, "Class", "ms-2");
                    if (_cancelText is not null)
                    {
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.AddContent(7, _cancelText);
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.AddContent(7, _buttonChildContent);
                        }));
                    }

                    builder.CloseComponent();
                    break;
                default: throw new NotImplementedException();
            }
        };

        public RenderFragment RenderProgress() => builder =>
        {
            switch (_type)
            {
                case MBButtonType.DEFAULT:
                    builder.OpenComponent<MudProgressCircular>(0);
                    builder.AddAttribute(1, "Indeterminate", true);
                    builder.AddAttribute(2, "Size", Size.Small);
                    builder.AddAttribute(3, "Class", "ms-n1 mr+2");
                    builder.CloseComponent();
                    builder.OpenComponent<MudText>(4);
                    builder.AddAttribute(5, "Class", "ms-2");
                    builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                    {
                        childBuilder.AddContent(7, _buttonChildContent);
                    }));
                    builder.CloseComponent();
                    break;
                default: throw new NotImplementedException();
            }
        };

        protected (string? StartIcon, string? EndIcon, string? Label) GetFabParamtersBase(ICommandAsyncBase command, string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant = null)
        {
            if (_iconForState is IconForState.None)
            {
                if (!string.IsNullOrEmpty(startIcon) && !string.IsNullOrEmpty(endIcon))
                {
                    throw new InvalidOperationException("Async FabButton can not have start and end icon set!");
                }

                if (string.IsNullOrEmpty(startIcon) && string.IsNullOrEmpty(endIcon))
                {
                    throw new InvalidOperationException("Async FabButton must have start or end icon set!");
                }

                if (string.IsNullOrEmpty(startIcon))
                {
                    _iconForState = IconForState.Start;
                }
                else
                {
                    _iconForState = IconForState.End;
                }

                _buttonLabel = label;
            }

            if (!command.Executing)
            {
                if (_iconForState is IconForState.Start)
                {
                    startIcon = null;
                }

                if (_iconForState is IconForState.End)
                {
                    endIcon = null;
                }

                label = _buttonLabel;

                _iconForState = IconForState.None;
            }
            else
            {
                if (command.CanCancel && _cancelText is null)
                {
                    var cancelIcon = iconVariant switch
                    {
                        MBIconVariant.Filled => Icons.Material.Filled.Cancel,
                        MBIconVariant.Outlined => Icons.Material.Outlined.Cancel,
                        MBIconVariant.Sharp => Icons.Material.Sharp.Cancel,
                        MBIconVariant.Rounded => Icons.Material.Rounded.Cancel,
                        MBIconVariant.TwoTone => Icons.Material.TwoTone.Cancel,
                        _ => Icons.Material.Filled.Cancel
                    };

                    if (_iconForState is IconForState.Start)
                    {
                        startIcon = cancelIcon;
                    }

                    if (_iconForState is IconForState.End)
                    {
                        endIcon = cancelIcon;
                    }
                }

                if (command.HasProgress || (command.CanCancel && _cancelText is not null))
                {
                    var progressIcon = iconVariant switch
                    {
                        MBIconVariant.Filled => Icons.Material.Filled.Refresh,
                        MBIconVariant.Outlined => Icons.Material.Outlined.Refresh,
                        MBIconVariant.Sharp => Icons.Material.Sharp.Refresh,
                        MBIconVariant.Rounded => Icons.Material.Rounded.Refresh,
                        MBIconVariant.TwoTone => Icons.Material.TwoTone.Refresh,
                        _ => Icons.Material.Filled.Refresh
                    };

                    if (_iconForState is IconForState.Start)
                    {
                        startIcon = progressIcon;
                    }

                    if (_iconForState is IconForState.End)
                    {
                        endIcon = progressIcon;
                    }

                    label = _cancelText;
                }
            }

            return (startIcon, endIcon, label);
        }

        protected string GetIconButtonParamtersBase(ICommandAsyncBase command, string icon, MBIconVariant? iconVariant = null)
        {
            if (!command.Executing)
            {
                if (_iconForState is IconForState.None)
                {
                    _buttonIcon = icon;
                    _iconForState = IconForState.Start;
                }
                else
                {
                    ArgumentNullException.ThrowIfNull(_buttonIcon);
                    icon = _buttonIcon;
                    _iconForState = IconForState.None;
                }
            }
            else
            {
                if (command.CanCancel)
                {
                    icon = iconVariant switch
                    {
                        MBIconVariant.Filled => Icons.Material.Filled.Cancel,
                        MBIconVariant.Outlined => Icons.Material.Outlined.Cancel,
                        MBIconVariant.Sharp => Icons.Material.Sharp.Cancel,
                        MBIconVariant.Rounded => Icons.Material.Rounded.Cancel,
                        MBIconVariant.TwoTone => Icons.Material.TwoTone.Cancel,
                        _ => Icons.Material.Filled.Cancel
                    };
                }
                else if (command.HasProgress)
                {
                    icon = iconVariant switch
                    {
                        MBIconVariant.Filled => Icons.Material.Filled.Refresh,
                        MBIconVariant.Outlined => Icons.Material.Outlined.Refresh,
                        MBIconVariant.Sharp => Icons.Material.Sharp.Refresh,
                        MBIconVariant.Rounded => Icons.Material.Rounded.Refresh,
                        MBIconVariant.TwoTone => Icons.Material.TwoTone.Refresh,
                        _ => Icons.Material.Filled.Refresh
                    };
                }
            }

            return icon;
        }
    }

    public class ButtonAsyncRX : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }

        private readonly ICommandAsync _command;
        private readonly Color _cancelColor;

        private ButtonAsyncRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText) :
            base(type, buttonColor, buttonChildContent, confirmExecution, beforeExecution, afterExecution, cancelText)
        {
            ArgumentNullException.ThrowIfNull(command);
            _command = command;
            _cancelColor = cancelColor ?? Color.Warning;
        }

        public static ButtonAsyncRX Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText)
        {
            return new ButtonAsyncRX(type, buttonColor, buttonChildContent, command, confirmExecution, beforeExecution, afterExecution, cancelColor, cancelText);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameters()
        {
            Disabled = !_command.CanExecute() && !(_command.Executing && _command.CanCancel);

            if (_command.Executing && _command.CanCancel)
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing && _command.HasProgress ? RenderProgress() : _buttonChildContent;
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, async () => await Execute(_command.Execute));
            }
        }

        public (string? StartIcon, string? EndIcon, string? Label) GetFabParamters(string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant = null)
        {
            return GetFabParamtersBase(_command, startIcon, endIcon, label, iconVariant);
        }

        public string GetIconButtonParamters(string icon, MBIconVariant? iconVariant = null)
        {
            return GetIconButtonParamtersBase(_command, icon, iconVariant);
        }

        private async Task Execute(Func<Task> execute)
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                Disabled = !_command.CanCancel;
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                await execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }

    public class ButtonAsyncPRX<T> : ButtonBaseAsyncRX
    {
        public EventCallback<MouseEventArgs>? OnClick { get; private set; }

        private readonly ICommandAsync<T> _command;
        private readonly Color _cancelColor;

        private ButtonAsyncPRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText) :
            base(type, buttonColor, buttonChildContent, confirmExecution, beforeExecution, afterExecution, cancelText)
        {
            ArgumentNullException.ThrowIfNull(command);

            _command = command;
            _cancelColor = cancelColor ?? Color.Warning;
        }

        public static ButtonAsyncPRX<T> Create(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, ICommandAsync<T>? command, Func<Task<bool>>? confirmExecution, Action? beforeExecution, Action? afterExecution, Color? cancelColor, string? cancelText)
        {
            return new ButtonAsyncPRX<T>(type, buttonColor, buttonChildContent, command, confirmExecution, beforeExecution, afterExecution, cancelColor, cancelText);
        }

        [MemberNotNull(nameof(OnClick))]
        public void SetParameters(T? parameter, Func<T?, CancellationToken, Task<T?>>? parameterAsyncTransformation)
        {
            if (_command.Executing && _command.CanCancel)
            {
                Color = _cancelColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = RenderCancel();
                }
                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, () => _command.Cancel());
                Disabled = false;
            }
            else
            {
                Color = _buttonColor;
                if (_type is not MBButtonType.FAB)
                {
                    ChildContent = _command.Executing && _command.HasProgress ? RenderProgress() : _buttonChildContent;
                }

                _command.SetParameter(parameter);

                if (parameterAsyncTransformation is not null)
                {
                    _command.SetParameterAsyncTransformation(p => parameterAsyncTransformation(p, _command.CancellationToken));
                }

                OnClick = EventCallback.Factory.Create<MouseEventArgs>(this, async () => await Execute(_command.Execute));
                Disabled = !_command.CanExecute();
            }
        }

        public (string? StartIcon, string? EndIcon, string? Label) GetFabParamters(string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant = null)
        {
            return GetFabParamtersBase(_command, startIcon, endIcon, label, iconVariant);
        }

        public string GetIconButtonParamters(string icon, MBIconVariant? iconVariant = null)
        {
            return GetIconButtonParamtersBase(_command, icon, iconVariant);
        }

        private async Task Execute(Func<Task> execute)
        {
            if (_beforeExecution is not null)
            {
                _beforeExecution();
            }

            bool canExecute = true;

            if (_confirmExecution is not null)
            {
                canExecute = await _confirmExecution();
            }

            if (canExecute)
            {
                await execute();
            }

            if (_afterExecution is not null)
            {
                _afterExecution();
            }
        }
    }
}
