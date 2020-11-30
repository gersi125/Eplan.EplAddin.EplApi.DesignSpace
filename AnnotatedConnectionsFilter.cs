using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eplan.EplApi.DataModel;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class AnnotatedConnectionsFilter : ICustomFilter
    {
        public bool IsMatching(StorableObject objectToCheck)
        {
            Connection c = (Connection)objectToCheck;
            if(c.EndSymbolReference is Function && c.StartSymbolReference is Function)
            {
                return true;
            }
            return false;
        }
    }
}
