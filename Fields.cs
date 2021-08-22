using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    public static class Fields
    {

        public static long PACKAGE_TYPE = 1L;
        public static long EPLAN_TOOL_ID = 38L;
        public static long ROOT_TYPE = 4L;
        public static int COLOR_FAIL = 3;
        public static int COLOR_OK = 0;

        public static long EPLAN_PCKG = 110L;
        public static long EPLAN_FUNCTION_TYPE = 111L;
        public static long EPLAN_CONNECTION_TYPE = 112L;
        public static long EPLAN_FUNCTION_TYPE_PCKG = 113L;
        public static long EPLAN_CONNECTION_TYPE_PCKG = 114L;
        public static long EPLAN_FUNCTION_TYPE_FEATURE_PCKG = 115L;
        public static long EPLAN_CONNECTION_TYPE_FEATURE_PCKG = 116L;
        public static long EPLAN_METAMODEL = 117L;

        public static String NAME = "name";
        public static String DT = "DeviceTag";
        public static String PLCADDRESS = "PLCAddress";
        public static String OUT = "Out";
        public static String IN = "In";
        public static String OUT_PIN = "OutPin";
        public static String IN_PIN = "InPin";
        public static String PKG_NAME = "name";

        public static String[] EVENTS = new String[] { "EsSwitchConnectionUpdate",
                                                       "XGedEditPropertiesAction",
                                                       "GfDlgMgrActionIGfWindGfDlgMgrActionIGfWind",
                                                       "GfDlgMgrActionIGfWind",
                                                       "XEsUndoAction",
                                                       "ParamFromCSharpEvent" };
    }
}
