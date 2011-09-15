//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using Lardite.RefAssistant.Extensions;
using Mono.Cecil;

namespace Lardite.RefAssistant.ObjectModel.Checkers
{
    /// <summary>
    /// Shared data for <see cref="ITypesChecker"/> checkers.
    /// </summary>
    [Serializable]
    sealed class CheckerSharedData : IDisposable
    {
        #region Fields

        private const string PublicKeyToken = "PublicKeyToken";

        private IList<string> _usedTypes;
        private IList<ProjectReference> _candidateReferences;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of <see cref="CheckerSharedData"/> class.
        /// </summary>
        public CheckerSharedData()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="CheckerSharedData"/> class.
        /// </summary>
        /// <param name="candidateReferences">The list of assemblies the candidates to remove.</param>
        public CheckerSharedData(IList<ProjectReference> candidateReferences)
            : this(null, candidateReferences)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="CheckerSharedData"/> class.
        /// </summary>
        /// <param name="usedTypes">Assemblies' used types.</param>
        /// <param name="candidateReferences">The list of assemblies the candidates to remove.</param>
        private CheckerSharedData(IList<string> usedTypes, IList<ProjectReference> candidateReferences)
        {
            _usedTypes = (usedTypes != null) ? usedTypes : new List<string>();
            _candidateReferences = (candidateReferences != null) ? candidateReferences : new List<ProjectReference>();
        }

        #endregion // .ctor

        #region IDisposable members

        /// <summary>
        /// Dispose object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing && _usedTypes != null)
            {
                _usedTypes.Clear();
                _usedTypes = null;
            }

            if (disposing && _candidateReferences != null)
            {
                _candidateReferences.Clear();
                _candidateReferences = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get assemblies' used types.
        /// </summary>
        public IList<string> UsedTypes
        {
            get
            {
                if (_usedTypes == null)
                    throw Error.ObjectDisposed(this);

                return _usedTypes;
            }
        }

        /// <summary>
        /// Get list of assemblies the candidates to remove.
        /// </summary>
        public IList<ProjectReference> CandidateReferences
        {
            get
            {
                if (_candidateReferences == null)
                    throw Error.ObjectDisposed(this);

                return _candidateReferences;
            }
        }

        /// <summary>
        /// Exists candidates references.
        /// </summary>
        public bool HasCandidateReferences
        {
            get
            {
                if (_candidateReferences == null)
                    throw Error.ObjectDisposed(this);

                return _candidateReferences.Count > 0;
            }
        }

        #endregion // Properties

        #region Public methods

        /// <summary>
        /// Adds type by full name to used types collection.
        /// </summary>
        /// <param name="typeFullName">The full name of type.</param>
        public void AddToUsedTypes(string typeFullName)
        {
            if (_usedTypes == null)
                throw Error.ObjectDisposed(this);

            if (string.IsNullOrEmpty(typeFullName))
                throw Error.ArgumentNull("typeFullName");

            if (!_usedTypes.Contains(typeFullName))
            {
                _usedTypes.Add(typeFullName);
            }
        }

        /// <summary>
        /// Check whether type exists in used types collection.
        /// </summary>
        /// <param name="typeFullName">The full name of type.</param>
        /// <returns>Returns true if type already exists in used types collection; otherwise false.</returns>
        public bool IsUsedTypeExists(string typeFullName)
        {
            return _usedTypes.Contains(typeFullName);
        }

        /// <summary>
        /// Remove references from candidates by metadata scope.
        /// </summary>
        /// <param name="scope">The metadata scope.</param>
        public void RemoveFromCandidates(IMetadataScope scope)
        {
            if (_candidateReferences == null)
                throw Error.ObjectDisposed(this);

            if (scope != null)
            {
                ProjectReference projectReference = GetFromCandidates(scope);
                RemoveFromCandidates(projectReference);
            }
        }

        /// <summary>
        /// Remove references from candidates.
        /// </summary>
        /// <param name="reference">The project reference.</param>
        public void RemoveFromCandidates(ProjectReference reference)
        {
            if (_candidateReferences == null)
                throw Error.ObjectDisposed(this);

            if (reference != null)
            {
                CandidateReferences.Remove(reference);
            }
        }

        #endregion // Public methods

        #region Private methods

        /// <summary>
        /// Gets project reference from candidates.
        /// </summary>
        /// <param name="scope">Scope.</param>
        /// <returns>Project reference.</returns>
        private ProjectReference GetFromCandidates(IMetadataScope scope)
        {
            string publicKeyToken = scope.GetScopePublicKeyToken();
            string assemblyName = scope.GetAssemblyName();

            return CandidateReferences.FirstOrDefault(
                item => string.Compare(item.Name, assemblyName, StringComparison.InvariantCultureIgnoreCase) == 0
                     && string.Compare(item.PublicKeyToken, publicKeyToken, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        #endregion // Private methods
    }
}
