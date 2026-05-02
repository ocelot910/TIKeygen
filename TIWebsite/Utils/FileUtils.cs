using Microsoft.JSInterop;
using System.Text;

namespace TIWebsite.Utils
{
    public static class FileUtils
    {
        public static async Task DownloadFileFromStream(IJSRuntime js, string filename, string content)
        {
            Stream fileStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            using var streamRef = new DotNetStreamReference(stream: fileStream);

            await js.InvokeVoidAsync("downloadFileFromStream", filename, streamRef);
        }
    }
}
