using AutoTest.Domain.Model;
using AutoTest.UI.UC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.WebBrowser
{
    class CSUCTestCaseSelector : IDisposable
    {
        private SynchronizationContext context;
        private UCTestCaseSelector selector;
        public event Action<string> OnPublishMsg;

        static CSUCTestCaseSelector()
        {

        }

        public CSUCTestCaseSelector(SynchronizationContext context, UCTestCaseSelector selector)
        {
            this.context = context;
            this.selector = selector;
        }

        public void Dispose()
        {
            this.OnPublishMsg = null;
        }

        public void ReTestCase(int caseid)
        {
            selector.ReTestCase(caseid);
        }
    }
}
