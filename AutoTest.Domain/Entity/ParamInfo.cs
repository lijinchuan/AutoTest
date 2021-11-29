using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain.Entity
{
    [Serializable]
    public class ParamInfo : ICloneable
    {
        public bool Checked
        {
            get;
            set;
        } = true;

        public string Name
        {
            get;
            set;
        } = "";

        public string Value
        {
            get;
            set;
        } = "";

        public string Desc
        {
            get;
            set;
        } = "";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
