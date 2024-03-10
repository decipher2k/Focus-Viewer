using CefSharp;

namespace Focus_Browser
{
    public interface IExtensionHandler
    {
        bool CanAccessBrowser(IExtension extension, IBrowser browser, bool includeIncognito, IBrowser targetBrowser);
        void Dispose();
        IBrowser GetActiveBrowser(IExtension extension, IBrowser browser, bool includeIncognito);
        bool GetExtensionResource(IExtension extension, IBrowser browser, string file, IGetExtensionResourceCallback callback);
        bool OnBeforeBackgroundBrowser(IExtension extension, string url, IBrowserSettings settings);
        bool OnBeforeBrowser(IExtension extension, IBrowser browser, IBrowser activeBrowser, int index, string url, bool active, IWindowInfo windowInfo, IBrowserSettings settings);
        void OnExtensionLoaded(IExtension extension);
        void OnExtensionLoadFailed(CefErrorCode errorCode);
        void OnExtensionUnloaded(IExtension extension);
    }
}