using System;
using System.Runtime.Serialization;

namespace Lardite.RefAssistant
{
    [Serializable]
    internal class ProjectNotFoundException : ApplicationException
    {
        private const string PROJECT_KEY = "project-key";

        public ProjectNotFoundException(string projectName)
            : this(projectName, string.Format(Resources.ProjectNotFoundException_Msg, projectName))
        {
        }
        
        public ProjectNotFoundException(string projectName, string message)
            : this(message, (Exception)null)
        {
            Data[PROJECT_KEY] = projectName;
        }

        public ProjectNotFoundException(string message, Exception inner)
            : base(message, inner)
        { 
        }

        protected ProjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { 
        }

        public string ProjectName
        {
            get
            {
                return Data[PROJECT_KEY] as string;
            }
        }
    }
}
