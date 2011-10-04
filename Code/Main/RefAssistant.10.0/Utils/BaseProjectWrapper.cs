//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com),
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;
using VSLangProj80;

using Lardite.RefAssistant.ObjectModel;

namespace Lardite.RefAssistant.Utils
{
    /// <summary>
    /// Base features to wrap visual studio project.
    /// </summary>
    internal class BaseProjectWrapper
    {
        #region Constants

        protected const string OutputPath = "OutputPath";
        protected const string LocalPath = "LocalPath";
        protected const string OutputFileName = "OutputFileName";

        #endregion // Constants

        #region Fields

        private Project _vsProject;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="BaseProjectWrapper"/> class.
        /// </summary>
        /// <param name="vsProject">The Visual Studio project.</param>
        public BaseProjectWrapper(Project vsProject)
        {
            _vsProject = vsProject;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get wrapped project.
        /// </summary>
        protected Project Project
        {
            get { return _vsProject; }
        }

        /// <summary>
        /// Get project kind.
        /// </summary>
        public Guid Kind
        {
            get { return Guid.Parse(Project.Kind); }
        }

        /// <summary>
        /// Get project information.
        /// </summary>
        public ProjectInfo ProjectInfo
        {
            get
            {
                return new ProjectInfo
                    {
                        Name = Project.Name,
                        Type = Kind,
                        AssemblyPath = GetOutputAssemblyPath(),
                        ConfigurationName = Project.ConfigurationManager.ActiveConfiguration.ConfigurationName,
                        PlatformName = Project.ConfigurationManager.ActiveConfiguration.PlatformName,
                        References = GetProjectReferences()
                    };
            }
        }

        /// <summary>
        /// Check wherether project is building currently.
        /// </summary>
        public bool IsBuildInProgress
        {
            get { return DTEHelper.IsBuildInProgress(Project); }
        }

        #endregion // Properties

        #region Virtual methods

        /// <summary>
        /// Get outout assembly path.
        /// </summary>
        /// <returns>Returns full path.</returns>
        public virtual string GetOutputAssemblyPath()
        {
            string outputPath = Project.ConfigurationManager.ActiveConfiguration.Properties.Item(OutputPath).Value.ToString();
            string buildPath = Project.Properties.Item(LocalPath).Value.ToString();
            string targetName = Project.Properties.Item(OutputFileName).Value.ToString();
            return Path.Combine(buildPath, Path.Combine(outputPath, targetName));
        }

        /// <summary>
        /// Removes unused usings from project classes.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public virtual void RemoveUnusedUsings(IServiceProvider serviceProvider)
        {
        }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        public virtual IEnumerable<ProjectReference> RemoveUnusedReferences(IEnumerable<ProjectReference> unusedProjectReferences)
        {
            var projectReferences = ((VSProject2)Project.Object).References.Cast<Reference3>();
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
        public virtual IEnumerable<ProjectReference> GetProjectReferences()
        {
            var references = new List<ProjectReference>();
            var projectReferences = ((VSProject2)Project.Object).References;
            foreach (Reference3 projectReference in projectReferences)
            {
                references.Add(CreateProjectReference(projectReference.Name, projectReference.Identity, projectReference.Path,
                    projectReference.Version, projectReference.Culture, projectReference.PublicKeyToken));
            }

            return references;
        }

        #endregion // Virtual methods

        #region Public methods

        /// <summary>
        /// Builds the project.
        /// </summary>
        /// <returns>Returns zero (0) if there are no errors.</returns>
        public int Build()
        {
            return DTEHelper.BuildProject(Project);
        }

        #endregion // Public methods

        #region Internal methods

        /// <summary>
        /// Creates project reference.
        /// </summary>
        protected ProjectReference CreateProjectReference(string name, string identity, string location, string version,
            string culture, string publicKeyToken)
        {
            return new ProjectReference()
                {
                    Name = name,
                    Identity = identity,
                    Location = location,
                    Version = version,
                    Culture = string.Compare(culture, "0", false, CultureInfo.InvariantCulture) == 0 ? string.Empty : culture,
                    PublicKeyToken = publicKeyToken
                };
        }

        #endregion // Internal methods
    }
}
