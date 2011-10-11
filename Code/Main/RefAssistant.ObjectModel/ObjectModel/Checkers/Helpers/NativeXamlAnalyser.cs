//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xaml.Tools.XamlDom;
using System.Xaml;

namespace Lardite.RefAssistant.ObjectModel.Checkers.Helpers
{
    sealed class NativeXamlAnalyser : IXamlAnalyser, IDisposable
    {
        #region Fields

        private XamlXmlReader _xamlReader;

        #endregion // Fields

        #region .ctor

        /// <summary>
        /// Initialize a new instance of the <see cref="Lardite.RefAssistant.ObjectModel.Checkers.Helpers.NativeXamlAnalyser"/> class.
        /// </summary>
        /// <param name="stream">The XAML stream.</param>
        public NativeXamlAnalyser(Stream stream)
        {
            _xamlReader = new XamlXmlReader(stream, new XamlXmlReaderSettings { CloseInput = true });
        }

        #endregion // .ctor

        #region IDisposable implementation

        /// <summary>
        /// Dispose object.
        /// </summary>
        public void Dispose()
        {
            if (_xamlReader != null)
            {
                _xamlReader.Close();
                _xamlReader = null;
            }
        }

        #endregion // IDisposable implementation

        #region IXamlAnalyser impementation

        /// <summary>
        /// Gets types list which declared into XAML markup.
        /// </summary>
        /// <returns>Returns <see cref="Lardite.RefAssistant.ObjectModel.Checkers.XamlTypeDeclaration"/> collection.</returns>
        public IEnumerable<XamlTypeDeclaration> GetDeclaredTypes()
        {
            XamlDomObject rootNode = XamlDomServices.Load(_xamlReader) as XamlDomObject;

            return (rootNode != null) 
                ? rootNode.DescendantsAndSelf().Select(t => new XamlTypeDeclaration(t.Type)) 
                : new List<XamlTypeDeclaration>();
        }

        #endregion // IXamlAnalyser impementation
    }
}
