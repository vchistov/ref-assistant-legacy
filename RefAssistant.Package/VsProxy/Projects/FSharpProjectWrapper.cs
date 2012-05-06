//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.FSharp.ProjectSystem.Automation;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.VsProxy.Projects
{
    /// <summary>
    /// Wraps F# project.
    /// </summary>
    internal class FSharpProjectWrapper : BaseProjectWrapper
    {
        #region Constants

        private const string FullPath = "FullPath";
        
        #endregion // Constants

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="FSharpProjectWrapper"/> class.
        /// </summary>
        /// <param name="vsProject">The Visual Studio project.</param>
        public FSharpProjectWrapper(Project vsProject)
            :base(vsProject)
        { }

        #endregion // .ctor

        #region Public methods

        /// <summary>
        /// Get outout assembly path.
        /// </summary>
        /// <returns>Returns full path.</returns>
        public override string GetOutputAssemblyPath()
        {
            string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item(OutputPath).Value.ToString();
            string fullPath = Project.Properties.Item(FullPath).Value.ToString();
            string targetName = Project.Properties.Item(OutputFileName).Value.ToString();
            return Path.Combine(fullPath, Path.Combine(outputPath, targetName));
        }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        public override IEnumerable<ProjectReference> RemoveUnusedReferences(IEnumerable<ProjectReference> unusedProjectReferences)
        {
            var projectReferences = ((OAVSProject)Project.Object).References.Cast<OAAssemblyReference>();
            var unusedReferences = from p in projectReferences
                                   join up in unusedProjectReferences on p.Name equals up.Name
                                   select new { Reference = p, ProjectReference = up };
            
            var unusedReferencesList = unusedReferences.Select(r => r.ProjectReference).ToList();
            foreach (var unusedReference in unusedReferences)
            {
                unusedReference.Reference.Remove();
            }

            return unusedReferencesList;
        }

        /// <summary>
        /// Get list of project references.
        /// </summary>
        /// <returns>Returns project references.</returns>
        public override IEnumerable<ProjectReference> GetProjectReferences()
        {
            var references = new List<ProjectReference>();
            var projectReferences = ((OAVSProject)Project.Object).References;
            foreach (OAAssemblyReference projectReference in projectReferences)
            {
                references.Add(BuildProjectReference(projectReference.Name, projectReference.Identity, projectReference.Path,
                    projectReference.Version, projectReference.Culture, projectReference.PublicKeyToken));
            }

            return references;
        }

        #endregion // Public methods
    }
}
