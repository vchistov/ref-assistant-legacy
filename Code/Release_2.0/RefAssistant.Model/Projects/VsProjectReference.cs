using System;

namespace Lardite.RefAssistant.Model.Projects
{
    public class VsProjectReference
    {
        public VsProjectReference(string name, string location, Version version, string culture = null, bool isSpecificVerions = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException("location");

            Name = name;
            Location = location;
            Version = version;
            Culture = culture;
            IsSpecificVersion = isSpecificVerions;
        }

        public string Name { get; private set; }

        public string Location { get; private set; }

        public Version Version { get; private set; }

        public string Culture { get; private set; }

        public bool IsSpecificVersion { get; private set; }
    }
}
