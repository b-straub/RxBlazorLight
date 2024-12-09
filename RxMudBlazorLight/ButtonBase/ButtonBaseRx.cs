using Microsoft.AspNetCore.Components;
using MudBlazor;
using RxBlazorLightCore;
using RxMudBlazorLight.Extensions;

namespace RxMudBlazorLight.ButtonBase
{
    internal class ButtonBaseRx
    {
        public bool Disabled { get; protected set; } = true;
        public Color Color { get; protected set; }
        public RenderFragment? ChildContent { get; protected set; }

        protected readonly MbButtonType ButtonType;
        protected readonly Color ButtonColor;
        protected readonly RenderFragment? ButtonChildContent;

        protected readonly Color? CancelColor;
        protected readonly string? CancelText;
        protected readonly bool HasProgress;
        internal readonly Guid ID = Guid.NewGuid();

        private enum IconForState
        {
            NONE,
            START,
            END
        }

        private IconForState _iconForState = IconForState.NONE;
        private string? _buttonLabel;
        private string? _buttonIcon;

        protected ButtonBaseRx(MbButtonType buttonType, Color buttonColor,
            RenderFragment? buttonChildContent, bool hasProgress, string? cancelText, Color? cancelColor)
        {
            ButtonType = buttonType;
            ButtonColor = buttonColor;
            Color = ButtonColor;

            ButtonChildContent = buttonChildContent;
            ChildContent = ButtonChildContent;

            HasProgress = hasProgress;
            CancelText = cancelText;
            CancelColor = cancelColor;
        }

        public RenderFragment RenderCancel() => builder =>
        {
            switch (ButtonType)
            {
                case MbButtonType.DEFAULT:
                    if (HasProgress)
                    {
                        builder.OpenComponent<MudProgressCircular>(0);
                        builder.AddAttribute(1, "Indeterminate", true);
                        builder.AddAttribute(2, "Size", Size.Small);
                        builder.AddAttribute(3, "Class", "ms-n1");
                        builder.CloseComponent();
                    }
                    builder.OpenComponent<MudText>(4);
                    if (HasProgress)
                    {
                        builder.AddAttribute(5, "Class", "ms-2");
                    }
                    if (CancelText is not null)
                    {
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.AddContent(7, CancelText);
                        }));
                    }
                    else
                    {
                        builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                        {
                            childBuilder.AddContent(7, ButtonChildContent);
                        }));
                    }

                    builder.CloseComponent();
                    break;
                default: throw new NotImplementedException();
            }
        };

        public RenderFragment RenderProgress() => builder =>
        {
            switch (ButtonType)
            {
                case MbButtonType.DEFAULT:
                    builder.OpenComponent<MudProgressCircular>(0);
                    builder.AddAttribute(1, "Indeterminate", true);
                    builder.AddAttribute(2, "Size", Size.Small);
                    builder.AddAttribute(3, "Class", "ms-n1 mr+2");
                    builder.CloseComponent();
                    builder.OpenComponent<MudText>(4);
                    builder.AddAttribute(5, "Class", "ms-2");
                    builder.AddAttribute(6, "ChildContent", (RenderFragment)((childBuilder) =>
                    {
                        childBuilder.AddContent(7, ButtonChildContent);
                    }));
                    builder.CloseComponent();
                    break;
                default: throw new NotImplementedException();
            }
        };

        public (string? StartIcon, string? EndIcon, string? Label) GetFabParameters(IStateCommandAsync stateCommand, string? startIcon, string? endIcon, string? label, MbIconVariant? iconVariant)
        {
            if (_iconForState is IconForState.NONE)
            {
                if (!string.IsNullOrEmpty(startIcon) && !string.IsNullOrEmpty(endIcon) && HasProgress)
                {
                    throw new InvalidOperationException("Async FabButton with progress can not have start and end icon set!");
                }

                if (string.IsNullOrEmpty(startIcon))
                {
                    _iconForState = IconForState.START;
                }
                else if (string.IsNullOrEmpty(endIcon))
                {
                    _iconForState = IconForState.END;
                }

                _buttonLabel = label;
            }

            if (!stateCommand.Changing() || stateCommand.ChangeCallerID != ID)
            {
                if (_iconForState is IconForState.START)
                {
                    startIcon = null;
                }

                if (_iconForState is IconForState.END)
                {
                    endIcon = null;
                }

                label = _buttonLabel;

                _iconForState = IconForState.NONE;
            }
            else
            {
                if (HasProgress)
                {
                    var progressIcon = iconVariant.GetProgressIcon();

                    if (_iconForState is IconForState.START)
                    {
                        startIcon = progressIcon;
                    }

                    if (_iconForState is IconForState.END)
                    {
                        endIcon = progressIcon;
                    }
                }

                if (CancelText is not null && stateCommand.CanCancel)
                {
                    label = CancelText;
                }
            }

            return (startIcon, endIcon, label);
        }

        public void SetButtonIcon(IStateCommandAsync stateCommand, string icon)
        {
            if (!stateCommand.Changing() || _buttonIcon is null)
            {
                _buttonIcon = icon;  
            }
        }
        
        public string GetIconButtonParameters(IStateCommandAsync stateCommand, MbIconVariant? iconVariant)
        {
            ArgumentNullException.ThrowIfNull(_buttonIcon);
            var icon = _buttonIcon;
            
            if (_iconForState is IconForState.NONE)
            {
                _iconForState = IconForState.START;
            }

            if (!stateCommand.Changing() || stateCommand.ChangeCallerID != ID)
            {
                _iconForState = IconForState.NONE;
            }
            else
            {
                if (HasProgress)
                {
                    icon = iconVariant.GetProgressIcon();
                }
            }

            return icon;
        }

        public string GetBadgeIcon(IStateCommandAsync stateCommand, MbIconVariant? iconVariant)
        {
            if (stateCommand.CanCancel && stateCommand.Changing() && stateCommand.ChangeCallerID == ID)
            {
                return iconVariant.GetCancelIcon();
            }

            return string.Empty;
        }

        protected void VerifyButtonParameters()
        {
            if (ButtonType is MbButtonType.DEFAULT || ButtonType is MbButtonType.FAB)
            {
                if (CancelText is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelText is provided!");
                }

                if (CancelColor is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelColor is provided!");
                }
            }
        }

        protected void VerifyButtonParametersAsync(IStateCommandAsync stateCommand)
        {
            if (ButtonType is MbButtonType.DEFAULT)
            {
                if (stateCommand.CanCancel && CancelText is null)
                {
                    throw new InvalidOperationException("Command can be cancelled, but no CancelText is provided!");
                }
            }

            if (ButtonType is MbButtonType.DEFAULT || ButtonType is MbButtonType.FAB)
            {
                if (!stateCommand.CanCancel && CancelText is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelText is provided!");
                }
            }

            if (ButtonType is MbButtonType.DEFAULT)
            {
                if (CancelText is null && CancelColor is not null)
                {
                    throw new InvalidOperationException("Command can not be cancelled, but CancelColor is provided!");
                }
            }
        }
    }
}
