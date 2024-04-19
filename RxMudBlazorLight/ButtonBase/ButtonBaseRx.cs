using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;
using System.Reflection.Emit;

namespace RxMudBlazorLight.ButtonBase
{
    internal class ButtonBaseRx
    {
        public bool Disabled { get; protected set; } = true;
        public Color Color { get; protected set; }
        public RenderFragment? ChildContent { get; protected set; }

        protected readonly MBButtonType _buttonType;
        protected readonly Color _buttonColor;
        protected readonly RenderFragment? _buttonChildContent;

        protected readonly Color? _cancelColor;
        protected readonly string? _cancelText;
        protected readonly bool _hasProgress;
        internal readonly Guid _id = Guid.NewGuid();

        private enum IconForState
        {
            None,
            Start,
            End
        }

        private IconForState _iconForState = IconForState.None;
        private string? _buttonLabel;
        private string? _buttonIcon;

        protected ButtonBaseRx(MBButtonType buttonType, Color buttonColor,
            RenderFragment? buttonChildContent, bool hasProgress, string? cancelText, Color? cancelColor)
        {
            _buttonType = buttonType;
            _buttonColor = buttonColor;
            Color = _buttonColor;

            _buttonChildContent = buttonChildContent;
            ChildContent = _buttonChildContent;

            _hasProgress = hasProgress;
            _cancelText = cancelText;
            _cancelColor = cancelColor;
        }

        public RenderFragment RenderCancel() => builder =>
        {
            switch (_buttonType)
            {
                case MBButtonType.DEFAULT:
                    if (_hasProgress)
                    {
                        builder.OpenComponent<MudProgressCircular>(0);
                        builder.AddAttribute(1, "Indeterminate", true);
                        builder.AddAttribute(2, "Size", Size.Small);
                        builder.AddAttribute(3, "Class", "ms-n1");
                        builder.CloseComponent();
                    }
                    builder.OpenComponent<MudText>(4);
                    if (_hasProgress)
                    {
                        builder.AddAttribute(5, "Class", "ms-2");
                    }
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
            switch (_buttonType)
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

        public (string? StartIcon, string? EndIcon, string? Label) GetFabParameters(IStateCommandAsync stateCommand, string? startIcon, string? endIcon, string? label, MBIconVariant? iconVariant)
        {
            if (_iconForState is IconForState.None)
            {
                if (!string.IsNullOrEmpty(startIcon) && !string.IsNullOrEmpty(endIcon) && _hasProgress)
                {
                    throw new InvalidOperationException("Async FabButton with progress can not have start and end icon set!");
                }

                if (string.IsNullOrEmpty(startIcon))
                {
                    _iconForState = IconForState.Start;
                }
                else if (string.IsNullOrEmpty(endIcon))
                {
                    _iconForState = IconForState.End;
                }

                _buttonLabel = label;
            }

            if (!stateCommand.Changing() || stateCommand.ChangeCallerID != _id)
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
                if (_hasProgress)
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
                }

                if (_cancelText is not null && stateCommand.CanCancel)
                {
                    label = _cancelText;
                }
            }

            return (startIcon, endIcon, label);
        }

        public string GetIconButtonParameters(IStateCommandAsync stateCommand, string icon, MBIconVariant? iconVariant)
        {
            if (_iconForState is IconForState.None)
            {
                _iconForState = IconForState.Start;
                _buttonIcon = icon;
            }

            if (!stateCommand.Changing() || stateCommand.ChangeCallerID != _id)
            {
                ArgumentNullException.ThrowIfNull(_buttonIcon);
                icon = _buttonIcon;
                _iconForState = IconForState.None;
            }
            else
            {
                if (_hasProgress)
                {
                    icon = iconVariant.GetProgressIcon();
                }
            }

            return icon;
        }

        public string GetBadgeIcon(IStateCommandAsync stateCommand, MBIconVariant? iconVariant)
        {
            if (stateCommand.CanCancel && stateCommand.Changing() && stateCommand.ChangeCallerID == _id)
            {
                return iconVariant.GetCancelIcon();
            }

            return string.Empty;
        }

        protected void VerifyButtonParameters()
        {
            if (_buttonType is MBButtonType.DEFAULT || _buttonType is MBButtonType.FAB)
            {
                if (_cancelText is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelText is provided!");
                }

                if (_cancelColor is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelColor is provided!");
                }
            }
        }

        protected void VerifyButtonParametersAsync(IStateCommandAsync stateCommand)
        {
            if (_buttonType is MBButtonType.DEFAULT)
            {
                if (stateCommand.CanCancel && _cancelText is null)
                {
                    throw new InvalidOperationException("Command can be cancelled, but no CancelText is provided!");
                }
            }

            if (_buttonType is MBButtonType.DEFAULT || _buttonType is MBButtonType.FAB)
            {
                if (!stateCommand.CanCancel && _cancelText is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelText is provided!");
                }
            }

            if (_buttonType is MBButtonType.DEFAULT)
            {
                if (_cancelText is null && _cancelColor is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelColor is provided!");
                }
            }
        }
    }
}
