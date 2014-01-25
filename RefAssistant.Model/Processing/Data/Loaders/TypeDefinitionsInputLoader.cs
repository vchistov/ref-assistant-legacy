using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Lardite.RefAssistant.Algorithms.Contracts;

namespace Lardite.RefAssistant.Model.Processing.Data.Loaders
{
    internal class TypeDefinitionsInputLoader : IAlgorithmInputLoader<IEnumerable<ITypeDefinition>>
    {
        public IEnumerable<ITypeDefinition> Load(IProject project)
        {
            Contract.Requires(project != null);
            Contract.Ensures(Contract.Result<IEnumerable<ITypeDefinition>>() != null);

            return project.Assembly.TypeDefinitions;
        }
    }
}
