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
            int no_connections = 0;

            var pr_name = project.ProjectName.ToString();
            var pr_type = project.TypeOfProject.ToString();
            var pagesLocationsGroups = project.Pages.GroupBy(obj => obj.Properties.DESIGNATION_LOCATION);
            var pagesDesignationsGroups = project.Pages.GroupBy(obj => obj.Properties.DESIGNATION_PLANT);
           
            //var prjSettings = new ProjectSettings(project).ToString();

            StreamWriter ws;
            FileInfo epinfo = new FileInfo(@"C:\Users\veshi\Documents\" + pr_name + "_Info.txt");
            ws = epinfo.AppendText();

            ws.WriteLine("Eplan Project Name: " + pr_name);
            ws.WriteLine("==============================================");
            ws.WriteLine("Project Type: " + pr_type);
            ws.WriteLine("==============================================");

            HashSet<Function> functions = new HashSet<Function>();

            
            foreach (var pageDesignation in pagesDesignationsGroups)
            {
                string designation = pageDesignation.Key;

                ws.WriteLine("Page Designation Plant: " + designation);
                

                foreach (var pageGroup in pagesLocationsGroups)
                {
                    string location = pageGroup.Key;

                    ws.WriteLine("\t- Page Location Group: " + location);
                    foreach (Page page in pageGroup)
                    {
                        var p_name = page.Properties.PAGE_NAME;
                        var p_type = page.Properties.PAGE_TYPE;
                        var p_desc = page.Properties.DESIGNATION_FUNCTIONALASSIGNMENT_DESCR;
                        var plc = page.PLCs;
                        for (int i = 0; i < page.Functions.Length; i++)
                        {
                            functions.Add(page.Functions[i]);
                        }

                        ws.WriteLine("\t\t- Page Name: " + p_name + " || Page Type: " + p_type);
                        for (int i = 0; i < plc.Length; i++)
                        {
                            ws.WriteLine("\t\t- Page PLC: " + plc[i].Name);
                            no_plcs++;
                        }

                        ws.WriteLine("\n");

                        foreach (Function f in functions)
                        {
                            ws.WriteLine("\t\t\t- Function: " + f.Name);
                            if (f.VisibleName == "") { }
                            else
                            {
                                ws.WriteLine("\t\t\t\t - Function Visible Name: " + f.VisibleName);
                            }

                            ws.WriteLine("\t\t\t\t - Function Type: " + f.Category + "\n");
                            ws.WriteLine("\t\t\t\t - Function Location X: " + f.Location.X);
                            ws.WriteLine("\t\t\t\t - Function Location Y: " + f.Location.Y + "\n");

                            for (int i = 0; i < f.Connections.Length; i++)
                            {
                                ws.WriteLine("\t\t\t\t - Connection fout X: " + f.Connections[i].StartSymbolReference.Location.X);
                                ws.WriteLine("\t\t\t\t - Connection fout Y: " + f.Connections[i].StartSymbolReference.Location.Y);
                                ws.WriteLine("\t\t\t\t - Connection fin Y: " + f.Connections[i].EndSymbolReference.Location.X);
                                ws.WriteLine("\t\t\t\t - Connection fin Y: " + f.Connections[i].EndSymbolReference.Location.Y + "\n");
                                no_connections++;
                            }

                            if (f.Properties.FUNC_DEVICETAG_FULLNAME == "") { }
                            else
                            {
                                ws.WriteLine("\t\t\t\t - Function DT: " + f.Properties.FUNC_DEVICETAG_FULLNAME);
                                ws.WriteLine("\n");
                            }

                            no_functions++;
                        }

                        no_pages++;
                    }
                }

                ws.WriteLine("==============================================");
                ws.WriteLine("Number of Pages:       " + no_pages);
                ws.WriteLine("Number of Functions:   " + no_functions);
                ws.WriteLine("Number of Connections: " + no_connections);
                ws.WriteLine("Number of PLCs:        " + no_plcs);
                ws.WriteLine("==============================================");
                ws.Dispose();
            }
        }
    }
}