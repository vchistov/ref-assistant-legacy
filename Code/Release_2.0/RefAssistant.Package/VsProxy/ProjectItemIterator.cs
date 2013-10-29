//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace Lardite.RefAssistant.VsProxy
{
    /// <summary>
    /// Iterator for project item hierachy.
    /// </summary>
    internal sealed class ProjectItemIterator : IEnumerable<ProjectItem>
    {
        private ProjectItems projectItems;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Items.</param>
        public ProjectItemIterator(ProjectItems items)
        {
            ThrowUtils.ArgumentNull(() => items);

            this.projectItems = items;
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns>Project items.</returns>
        public IEnumerator<ProjectItem> GetEnumerator()
        {
            return (Enumerate(projectItems).Select(item => item)).GetEnumerator();
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerate items.
        /// </summary>
        /// <param name="items">Project items.</param>
        /// <returns>All project items.</returns>
        private IEnumerable<ProjectItem> Enumerate(ProjectItems items)
        {
            foreach (ProjectItem item in items)
            {
                yield return item;

                foreach (ProjectItem subItem in Enumerate(item.ProjectItems))
                {
                    yield return subItem;
                }
            }
        }
    }
}