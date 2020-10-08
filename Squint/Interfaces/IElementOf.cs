using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquintScript.Interfaces
{
    interface IContains<T>
    {
        bool Contains(T element);
    }
}
