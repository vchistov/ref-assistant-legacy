using System;

namespace Lardite.RefAssistant.Model.Projects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProjectKindAttribute : Attribute
    {
        public ProjectKindAttribute(ProjectKinds kind, string guid)
        {
            this.ProjectKind = kind;
            this.Guid = Guid.Parse(guid);
        }

        public ProjectKinds ProjectKind { get; private set; }

        public Guid Guid { get; private set; }
    }
}
