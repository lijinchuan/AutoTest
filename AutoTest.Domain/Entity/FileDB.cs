using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    public class FileDB
    {
        public int Id
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public string FileContent
        {
            get;
            set;
        }

        public DateTime CDate
        {
            get;
            set;
        }

        public DateTime MDate
        {
            get;
            set;
        }
    }
}
