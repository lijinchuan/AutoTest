using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser
{
    public class CommonFileDialogHandler : IDialogHandler
    {
        public Func<List<string>> GetFiles;

        public bool OnFileDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, CefFileDialogMode mode, string title, string defaultFilePath, IReadOnlyCollection<string> acceptFilters, IReadOnlyCollection<string> acceptExtensions, IReadOnlyCollection<string> acceptDescriptions, IFileDialogCallback callback)
        {
            var files = GetFiles?.Invoke();
            if (files != null && files.Count > 0)
            {
                GetFiles = null;
                callback.Continue(files);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
