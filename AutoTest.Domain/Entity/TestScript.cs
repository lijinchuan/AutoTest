﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class TestScript:IUpdate,ISearch
    {
        public static readonly string Index3 = $"{nameof(SourceId)}_{nameof(SiteId)}_{nameof(ScriptName)}";

        public int Id
        {
            get;
            set;
        }

        public int SourceId
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        public string ScriptName
        {
            get;
            set;
        }

        public string Desc
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }

        public bool Enable
        {
            get;
            set;
        }

        public int CompareTo(object obj)
        {
            if (obj is TestScript)
            {
                return this.Id.CompareTo(((TestScript)obj).Id);
            }

            return 1;
        }

        public string GetDisplayText()
        {
            return ScriptName;
        }

        public IComparable GetParentUpdate()
        {
            if (SiteId == 0)
            {
                return new TestSource
                {
                    Id=SourceId
                };
            }
            else
            {
                return new TestSite
                {
                    Id=SiteId
                };
            }
        }

        public bool Search(string wd)
        {
            return (this.Body ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1
                || (this.Desc ?? string.Empty).IndexOf(wd, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
