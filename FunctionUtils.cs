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
    class FunctionUtils
    {
        public static List<Function> GetAddedArtifactsAsFunctions(List<Function> functionList, List<Function> tempList)
        {
            return functionList.Except(tempList).ToList();
        }

        public static Boolean IsAnnotated(Function f)
        {
            return !f.Properties.FUNC_CUSTOM_SUPPLEMENTARYFIELD01.IsEmpty;
        }

        public static List<Connection> GetConnections(Function f) 
        {
            List<Connection> connections = new List<Connection>();
            foreach(Connection c in f.Connections)
            {
                Function startFunction = (Function)c.StartSymbolReference;
                Function endFunction = (Function)c.EndSymbolReference;
                if(FunctionUtils.IsAnnotated(startFunction) && FunctionUtils.IsAnnotated(endFunction))
                {
                    connections.Add(c);
                }          
            }
            return connections;
        }


        public static List<Function> GetAnnotatedFunctions(Project pr)
        {
            DMObjectsFinder objectFinder = new DMObjectsFinder(pr);
            AnnotationFilter annotationFilter = new AnnotationFilter();
            Function[] soa = objectFinder.GetFunctionsWithCF(annotationFilter);

            foreach(Function f in soa)
            {
                Debug.WriteLine("function x: " + f.Location.X);
                Debug.WriteLine("function y: " + f.Location.Y);
            }

            return soa.ToList<Function>();
        }

        internal static String GetFunctionName(Function f)
        {
            return f.Properties.FUNC_CUSTOM_SUPPLEMENTARYFIELD100.ToMultiLangString().GetStringToDisplay(Eplan.EplApi.Base.ISOCode.Language.L___);
        }

        public static string GetFunctionDT(Function function)
        {
            string DT = function.Properties.FUNC_DEVICETAG_FULLNAME;
            return DT;
        }
    }
}
