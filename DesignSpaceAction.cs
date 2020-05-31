using System;
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
            Project[] projects         = new ProjectManager().OpenProjects;
            ProjectManager pm          = new ProjectManager();
            WriteDownProject writedown = new WriteDownProject();

            foreach (var project in projects)
            {
                if (!project.IsExclusive)
                {
                    throw new Exception($"Project is not exclusive: {project.ProjectLinkFilePath}");
                }

                writedown.WriteDownPages(project);
            }

            return true;
        }
    }
       
}