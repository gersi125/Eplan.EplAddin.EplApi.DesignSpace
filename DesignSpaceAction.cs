using System;
using System.Collections.Generic;
using System.Linq;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;


namespace Eplan.EplAddin.EplApi.DesignSpace
{
    class DesignSpaceAction : IEplAction
    {
        public void GetActionProperties(ref ActionProperties actionProperties)
        { }

        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = "DesignSpaceAction"; //MethodBase.GetCurrentMethod().DeclaringType.Name;
            Ordinal = 20;
            return true;
        }

        bool IEplAction.Execute(ActionCallingContext oActionCallingContext)
        {
            Project[] projects = new ProjectManager().OpenProjects;
            ProjectManager pm = new ProjectManager();

            foreach (var project in projects)
            {
                if (!project.IsExclusive)
                {
                    throw new Exception($"Project is not exclusive: {project.ProjectLinkFilePath}");
                }

                //WriteDownPages(project);
                String strElkPath = project.ProjectLinkFilePath;
                String prName = project.ProjectName;

                pm.CopyProject(strElkPath, @"C:\TEMP\" + prName + ".elk", ProjectManager.CopyMode.All);

            }
            return true;
        }
    }
}


       /* private void WriteDownPages(Project project)
        {
            var pagesGroups = project.Pages.GroupBy(obj => obj.Properties.DESIGNATION_LOCATION);

            foreach (var pageGroup in pagesGroups)
            {
                string location = pageGroup.Key;
                Console.WriteLine("+ " + location);
                HashSet<string> functions = new HashSet<string>();
                HashSet<string> func_descs = new HashSet<string>();
                HashSet<string> plcs = new HashSet<string>();

                foreach (Page page in pageGroup)
                {
                    var function = page.Properties.DESIGNATION_PLANT;
                    var func_desc = page.ToStringIdentifier();
                    var plc = page.PLCs.ToString();

                    functions.Add(function);
                    func_descs.Add(func_desc);
                    plcs.Add(plc);
                }

                foreach (var function in functions)
                {
                    foreach (var func_desc in func_descs)
                    {   
                        Console.WriteLine("   =" + function + "(" + func_desc + ")");
                        foreach (var plc in plcs)
                        {
                            Console.WriteLine("PLC: " + plc);
                        }
                    }
                }

            }*/