using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class CommonUtils
    {
        public static string GetPLCAddress(Function function)
        {
            String plcAddress = null;
            try 
            {
                plcAddress = function.Properties.FUNC_PLCADDRESS;
                Debug.WriteLine("\t Start PLCADDRESS " + plcAddress);
            }
            catch (PropertyNotFoundException e)
            {
                Debug.WriteLine("No PLC => not a PLC Address");
            }

            return plcAddress;
        }

        public static Eplan.EplApi.Base.MultiLangString GetMultilangString(String str)
        {
            Eplan.EplApi.Base.MultiLangString ml = new Eplan.EplApi.Base.MultiLangString();
            ml.SetAsString(str);
            return ml;
        }
    }
}
