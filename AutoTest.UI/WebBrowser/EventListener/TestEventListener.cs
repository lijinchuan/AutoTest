using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI.WebBrowser.EventListener
{
    public class TestEventListener : EventListener
    {
        public override string GetEventName()
        {
            return nameof(TestEventListener);
        }
    }
}
