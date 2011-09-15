//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//         Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using VSLangProj;
using VSLangProj80;

using Lardite.RefAssistant.ObjectModel;
using Lardite.RefAssistant.UI;
using Lardite.RefAssistant.Utils;
using System.IO;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Shell gateway.
    /// </summary>
    public sealed class ShellGateway : IShellGateway
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly IExtensionOptions _options;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="options">Extension options.</param>
        public ShellGateway(IServiceProvider serviceProvider, IExtensionOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        #endregion // .ctor

        #region Properties

        /// <summary>
        /// Get DTE object.
        /// </summary>
        private DTE DTE
        {
            get { return (DTE)_serviceProvider.GetService(typeof(DTE)); }
        }

        /// <summary>
        /// Get DTE2 object.
        /// </summary>
        private DTE2 DTE2
        {
            get { return (DTE2)_serviceProvider.GetService(typeof(DTE)); }
        }

        #endregion // Properties

        #region Public methods

        /// <summary>
        /// Gets active project info.
        /// </summary>
        /// <returns>Active project info.</returns>
        public ProjectInfo GetActiveProjectInfo()
        {
            Project project = DTEHelper.GetActiveProject(_serviceProvider);
            if (project == null)
                return null;

            string projectAssemblyPath;
            if (DTEHelper.BuildProject(project, out projectAssemblyPath) != 0)
            {
                DTEHelper.ShowErrorList(_serviceProvider);
                return null;
            }

            return CreateProjectInfo(project, projectAssemblyPath);
        }

        /// <summary>
        /// Can show unused references window.
        /// </summary>
        /// <returns>If true, then can.</returns>
        public bool CanShowUnusedReferencesWindow
        {
            get
            {
                return _options.IsShowUnusedReferencesWindow.HasValue
                    && _options.IsShowUnusedReferencesWindow.Value;
            }
        }

        /// <summary>
        /// Shows unused references window.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>If true, then continue.</returns>
        public bool ShowUnusedReferencesWindow(ref IEnumerable<ProjectReference> unusedProjectReferences)
        {
            var window = new UnusedReferencesWindow();
            window.SetProjectReferences(unusedProjectReferences);
            window.IsShowThisWindowAgain = _options.IsShowUnusedReferencesWindow.Value;
            var result = window.ShowModal();
            if (result.HasValue && result.Value)
            {
                unusedProjectReferences = window.GetUnusedReferences();
                _options.IsShowUnusedReferencesWindow = window.IsShowThisWindowAgain;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        public bool CanRemoveUnusedReferences
        {
            get
            {
                var project = DTEHelper.GetActiveProject(_serviceProvider);
                return (project != null &&
                    Guid.Parse(project.Kind) != ProjectKinds.Modeling &&
                    Guid.Parse(project.Kind) != ProjectKinds.Database &&
                    !DTEHelper.IsBuildInProgress(project));
            }
        }

        /// <summary>
        /// Removes unused references.
        /// </summary>
        /// <param name="unusedProjectReferences">Unused project references.</param>
        /// <returns>Removed references count.</returns>
        public int RemoveUnusedReferences(IEnumerable<ProjectReference> unusedProjectReferences)
        {
            var project = DTEHelper.GetActiveProject(_serviceProvider);
            if (project == null)
                return 0;

            var projectReferences = ((VSProject2)project.Object).References.Cast<Reference3>();

            var unusedReferences = from p in projectReferences
                                   join up in unusedProjectReferences on p.Name equals up.Name
                                   select new { Reference = p, FullName = up.FullName };

            int amount = unusedReferences.Count();
            StringBuilder builder = new StringBuilder();
            foreach (var unusedReference in unusedReferences)
            {
                unusedReference.Reference.Remove();
                builder.AppendLine("  " + unusedReference.FullName);
            }
            LogManager.OutputLog.Information(builder.ToString());
            return amount;
        }

        /// <summary>
        /// Can remove unused references.
        /// </summary>
        /// <returns>If true, then can.</returns>
        public bool CanRemoveUnusedUsings
        {
            get
            {
                var project = DTEHelper.GetActiveProject(_serviceProvider);
                return (project != null
                    && Guid.Parse(project.Kind) == ProjectKinds.CSharp
                    && !DTEHelper.IsBuildInProgress(project)
                    && _options.IsRemoveUsingsAfterRemoving.HasValue
                    && _options.IsRemoveUsingsAfterRemoving.Value);
            }
        }

        /// <summary>
        /// Removes unused usings.
        /// </summary>
        public void RemoveUnusedUsings()
        {
            var project = DTEHelper.GetActiveProject(_serviceProvider);
            if (project != null)
            {
                LogManager.OutputLog.Information("  " + Resources.RefAssistantPackage_RemoveUnusedUsings);
                DTEHelper.RemoveUnusedUsings(project, _serviceProvider);
            }
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Creates project info.
        /// </summary>
        /// <param name="project">Project.</param>
        /// <param name="assemblyPath">Assembly path.</param>
        /// <returns>Project info.</returns>
        private ProjectInfo CreateProjectInfo(Project project, string assemblyPath)
        {
            var references = new List<ProjectReference>();
            References projectReferences = ((VSProject2)project.Object).References;
            foreach (Reference3 projectReference in projectReferences)
            {
                ProjectReference reference = new ProjectReference()
                {
                    Name = projectReference.Name,
                    Identity = projectReference.Identity,
                    Location = projectReference.Path,
                    Version = projectReference.Version,
                    Culture = string.Compare(projectReference.Culture, "0", false, CultureInfo.InvariantCulture) == 0 ? string.Empty : projectReference.Culture,
                    PublicKeyToken = projectReference.PublicKeyToken
                };
                references.Add(reference);
            }

            var projectInfo = new ProjectInfo
            {
                Name = project.Name,
                Type = Guid.Parse(project.Kind),
                AssemblyPath = assemblyPath,
                ConfigurationName = project.ConfigurationManager.ActiveConfiguration.ConfigurationName,
                PlatformName = project.ConfigurationManager.ActiveConfiguration.PlatformName,
                References = references
            };

            return projectInfo;
        }
        
        #endregion // Private methods
    }
}