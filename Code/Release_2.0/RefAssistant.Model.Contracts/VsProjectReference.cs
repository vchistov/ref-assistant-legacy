using System;

namespace Lardite.RefAssistant.Model.Contracts
{
    public class VsProjectReference : IEquatable<VsProjectReference>
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

        public bool IsActiveX { get; set; }

        [Obsolete]
        public string PublicKeyToken { get; set; }

        public bool Equals(VsProjectReference other)
        {
            if (other == null)
                return false;

            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }
    }
}
