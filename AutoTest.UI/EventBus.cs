using AutoTest.Domain;
using AutoTest.Domain.Entity;
using AutoTest.UI.WebTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.UI
{
    public class EventBus
    {
        public static Action<int> SelectTestCaseAction
        {
            get;
            set;
        }

        public static Action<TestResult> NotifyTestResultAction
        {
            get;
            set;
        }

        public static Action<IWebTask> NotifyTestStartAction
        {
            get;
            set;
        }

        public static Action<IComparable> NotifyTestThingChangeAction
        {
            get;
            set;
        }
    }
}
