
using Eplan.EplApi.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class AnnotationFilter : ICustomFilter
    {

        public bool IsMatching(StorableObject objectToCheck)
        {
            if (objectToCheck is Function)//objectToCheck.GetType() == new Function().GetType()
            {
                Function f = (Function)objectToCheck;
                return true;
            }
            return false;
        }
    }
}
