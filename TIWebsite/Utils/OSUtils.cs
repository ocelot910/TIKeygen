using Microsoft.JSInterop;

namespace TIWebsite.Utils
{
    public static class OSUtils
    {
        public static async Task<string> GetUserAgent(IJSRuntime js) => await js.InvokeAsync<string>("getUserAgent");

        public enum Platform
        {
            Windows, Mac, Linux, Unknown
        }

        public static Platform GetPlatform(string userAgent)
            => Enum.GetValues<Platform>()
                .FirstOrDefault(e => userAgent.Contains(e.ToString(), StringComparison.OrdinalIgnoreCase), Platform.Unknown);
    }
}
