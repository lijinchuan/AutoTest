using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class TaskBagLog
    {
        public int Id
        {
            get;
            set;
        }

        public int TaskBagId
        {
            get;
            set;
        }

        public DateTime LastTime
        {
            get;
            set;
        }
    }
}
