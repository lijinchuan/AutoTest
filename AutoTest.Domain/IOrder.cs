using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Domain
{
    public interface IOrder
    {
        string GetEidtName();

        int GetOrder();

        void SetOrder(int order);

        void SaveOrder(int order);
    }
}
