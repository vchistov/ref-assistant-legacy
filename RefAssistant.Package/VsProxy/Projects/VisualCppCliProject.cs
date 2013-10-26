﻿using System;
using EnvDTE;
using Lardite.RefAssistant.VsProxy.Projects.References;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    internal sealed class VisualCppCliProject : VsBaseProject
    {
        public VisualCppCliProject(Project project)
            : base(project, (vsp) => new Reference3Controller(vsp))
        {
        }

        public override string OutputAssemblyPath
        {
            get
            {
                var primaryOutput = Project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Primary Output");
                if (primaryOutput != null && primaryOutput.FileCount > 0)
                {
                    var url = ((object[])primaryOutput.FileURLs)[0].ToString();
                    return new Uri(url).LocalPath;
                }
                return string.Empty;
            }
        }

        //private const string ManagedExtensions = "ManagedExtensions";
        //public bool IsManaged
        //{
        //    get
        //    {
        //        try
        //        {
        //            // me is Microsoft.VisualStudio.VCProject.compileAsManagedOptions enum
        //            var me = (int)Project.ConfigurationManager
        //                .ActiveConfiguration.Properties.Item(ManagedExtensions).Value;
        //            return me != 0; // not equals "managedNotSet"
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //}
    }
}
