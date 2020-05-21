using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eplan.EplApi.ApplicationFramework;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    public class AddInModule : IEplAddIn
    {
        bool IEplAddIn.OnExit()
        {
            return true;
        }

        bool IEplAddIn.OnInit()
        {
            return true;
        }

        bool IEplAddIn.OnInitGui()
        {
            return true;

        }

        bool IEplAddIn.OnRegister(ref bool bLoadOnStart)
        {
            bLoadOnStart = true;
            return true;

        }

        bool IEplAddIn.OnUnregister()
        {
            return true;
        }
    }
}
