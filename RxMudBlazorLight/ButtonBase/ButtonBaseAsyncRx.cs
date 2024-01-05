using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;

namespace RxMudBlazorLight.ButtonBase
{
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

        protected ButtonBaseAsyncRX(MBButtonType type, Color buttonColor, RenderFragment? buttonChildContent, Action? beforeExecution, Action? afterExecution, string? cancelText) :
           base(type, beforeExecution, afterExecution)
        {
            _buttonColor = buttonColor;
            Color = _buttonColor;

            _buttonChildContent = buttonChildContent;
            ChildContent = _buttonChildContent;
            _cancelText = cancelText;

            if (type is MBButtonType.DEFAULT || type is MBButtonType.MENU)
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

        protected (string? StartIcon, string? EndIcon, string? Label) GetFabParametersBase(ICommandAsyncBase command, string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant = null)
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

            if (!command.Executing())
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
                if (command.CanCancel() && _cancelText is null)
                {
                    var cancelIcon = iconVariant.GetCancelIcon();

                    if (_iconForState is IconForState.Start)
                    {
                        startIcon = cancelIcon;
                    }

                    if (_iconForState is IconForState.End)
                    {
                        endIcon = cancelIcon;
                    }
                }

                if (command.HasProgress() && (!command.CanCancel() || _cancelText is not null))
                {
                    var progressIcon = iconVariant.GetProgressIcon();

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

        protected string GetIconButtonParametersBase(ICommandAsyncBase command, string icon, MBIconVariant? iconVariant = null)
        {
            if (!command.Executing())
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
                if (command.CanCancel())
                {
                    icon = iconVariant.GetCancelIcon();
                }
                else if (command.HasProgress())
                {
                    icon = iconVariant.GetProgressIcon();
                }
            }

            return icon;
        }
    }
}
