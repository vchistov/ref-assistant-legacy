//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Lardite.RefAssistant.ObjectModel
{
    /// <summary>
    /// Project reference resolver.
    /// </summary>
    [Serializable]
    sealed class ReferenceInspector : IDisposable
    {
        #region Fields

        private const string DomainFriendlyName = "Executor";
        private readonly string CheckExecutorTypeName = typeof(CheckExecutor).FullName;
        
        [NonSerialized]
        private AppDomain _executorDomain;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReferenceInspector()
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _executorDomain = AppDomain.CreateDomain(DomainFriendlyName, null, setup);
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
            if (disposing && _executorDomain != null)
            {
                AppDomain.Unload(_executorDomain);
                _executorDomain = null;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Inspect references.
        /// </summary>
        /// <param name="evaluator">The project evaluator.</param>
        /// <returns>Not used project references.</returns>
        public IEnumerable<ProjectReference> Inspect(IProjectEvaluator evaluator)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                var checkExecutor = (CheckExecutor)_executorDomain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().Location,
                    CheckExecutorTypeName,
                    true,
                    BindingFlags.CreateInstance,
                    null,
                    null,
                    null,                       
                    null);
                return checkExecutor.Execute(evaluator);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Reflection only assembly resolve.
        /// </summary>
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.Load(args.Name);
        }

        #endregion
    }
}