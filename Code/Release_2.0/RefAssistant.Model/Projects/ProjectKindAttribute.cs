using System;

namespace Lardite.RefAssistant.Model.Projects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProjectKindAttribute : Attribute
    {
        public ProjectKindAttribute(VsProjectKinds kind, string guid="")
        {
            this.Kind = kind;
            this.Guid = string.IsNullOrWhiteSpace(guid) ? Guid.Empty : Guid.Parse(guid);
        }

        public VsProjectKinds Kind { get; private set; }

        public Guid Guid { get; private set; }
    }
}
