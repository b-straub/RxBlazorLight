
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace RxMudBlazorLightTests.Extensions
{
    internal static class TestExtensions
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        public static async Task ClickAsync(this IRenderedFragment fragment, string id)
        {
            await fragment.FindID(id).ClickAsync(new MouseEventArgs());
        }

        public static void Click(this IRenderedFragment fragment, string id)
        {
            fragment.FindID(id).Click();
        }

        public static void VerifyTextContent(this IRenderedFragment fragment, string id, string text)
        {
            fragment.WaitForState(() => fragment.FindID(id).TextContent.Equals(text, StringComparison.InvariantCultureIgnoreCase), DefaultTimeout);
        }

        public static void VerifyMudChecked(this IRenderedFragment fragment, IElement? element)
        {
            fragment.WaitForState(() => element is not null && !element.HasAttribute("mud-checked"), DefaultTimeout);
        }

        public static void VerifyEnabled(this IRenderedFragment fragment, string id)
        {
            fragment.WaitForState(() => !fragment.FindID(id).HasAttribute("disabled"), DefaultTimeout);
        }

        public static IElement FindID(this IRenderedFragment fragment, string id)
        {
            return fragment.Find($"#{id.ToLowerInvariant()}");
        }

        public static IElement? FindMudRadioButton(this IRenderedFragment fragment, string radioId, string buttonText)
        {
            var buttons = fragment.FindID(radioId).QuerySelectorAll(".mud-button-root");
            return buttons.FirstOrDefault(c => c.ParentElement is not null && c.ParentElement.TextContent.Contains(buttonText, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
