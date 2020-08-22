using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;
using System.IO;

namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class WriteDownProject
    {
        public void WriteDownPages(Project project)
        {
            int no_pages = 0;
            int no_functions = 0;
            int no_plcs = 0;

            var pr_name = project.ProjectName.ToString();
            var pr_type = project.TypeOfProject.ToString();
            var pagesGroups = project.Pages.GroupBy(obj => obj.Properties.DESIGNATION_LOCATION);
            //var prjSettings = new ProjectSettings(project).ToString();

            StreamWriter ws;
            FileInfo epinfo = new FileInfo(@"C:\TEMP\" + pr_name + "_Info.txt");
            ws = epinfo.AppendText();

            ws.WriteLine("Eplan Project Name: " + pr_name);
            ws.WriteLine("==============================================");
            ws.WriteLine("Project Type: " + pr_type);
            ws.WriteLine("==============================================");
            //ws.WriteLine("Project Settings: " + prjSettings);  
            
            HashSet<string> p_names    = new HashSet<string>();
            HashSet<string> p_types    = new HashSet<string>();
            HashSet<string> p_props    = new HashSet<string>();
            HashSet<string> plcs       = new HashSet<string>();
            HashSet<string> plcs_ranks = new HashSet<string>();
            HashSet<string> functions  = new HashSet<string>();
            HashSet<string> func_descs = new HashSet<string>();


            DMObjectsFinder objectFinder = new DMObjectsFinder(project);
            AnnotationFilter annotationFilter = new AnnotationFilter();
            Function[] soa = objectFinder.GetFunctionsWithCF(annotationFilter);

            foreach (Function f in soa)
            {
                ws.WriteLine("function x:" + f.Location.X);
                ws.WriteLine("function y:" + f.Location.Y);
            }


            foreach (var pageGroup in pagesGroups)
            {
                string location = pageGroup.Key;

                foreach (Page page in pageGroup)
                {
                    var function = page.Properties.DESIGNATION_PLANT;
                    var func_desc = page.Properties.DESIGNATION_DOCTYPE;
                    var plc = page.PLCs.ToList().ToString();
                    var p_name = page.Properties.PAGE_NAME.ToString();
                    var p_type = page.Properties.PAGE_TYPE.ToString();

                    p_names.Add(p_name);
                    p_types.Add(p_type);
                    functions.Add(function);
                    func_descs.Add(func_desc);
                    plcs.Add(plc);
                }
            }


            foreach (var p_name in p_names)
            {
                foreach (var p_type in p_types)
                {
                    ws.WriteLine("Page: " + p_name + " || Page Type: " + p_type + "\n");

                    foreach (var function in functions)
                    {
                        foreach (var func_desc in func_descs)
                        {
                            ws.WriteLine(" - Function: " + function + " (" + func_desc + ")");
                            foreach (var plc in plcs)
                            {
                                ws.WriteLine("  - PLC: " + plc + "\n");
                              
                                no_plcs++;
                            }
                        }
                        no_functions++;
                    }
                    no_pages++;
                }
            }

            ws.WriteLine("==============================================");
            ws.WriteLine("Number of Pages:     " + no_pages);
            ws.WriteLine("Number of Functions: " + no_functions);
            ws.WriteLine("Number of PLCs:      " + no_plcs);
            ws.WriteLine("==============================================");
            ws.Dispose();

        }
    }
}