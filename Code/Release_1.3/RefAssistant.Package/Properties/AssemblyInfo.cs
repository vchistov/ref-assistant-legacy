//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com),
//         Belikov Sergey (sbelikov@lardite.com)
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if VS10
[assembly: AssemblyTitle("Lardite.RefAssistant.10.0.dll")]
[assembly: AssemblyDescription("Lardite Reference Assistant for Visual Studio 2010")]
[assembly: AssemblyProduct("Lardite Reference Assistant for Visual Studio 2010")]
[assembly: Guid("CA8E8309-7ED1-4F8C-A768-7A8CAE5D165E")]
#elif VS11
[assembly: AssemblyTitle("Lardite.RefAssistant.11.0.dll")]
[assembly: AssemblyDescription("Lardite Reference Assistant for Visual Studio 2012")]
[assembly: AssemblyProduct("Lardite Reference Assistant for Visual Studio 2012")]
[assembly: Guid("16F6FC93-74AB-4348-87E6-426148BF8227")]
#endif

[assembly: AssemblyCompany("Lardite Group")]
[assembly: AssemblyCopyright("Copyright © 2011-2012 Lardite.")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en", UltimateResourceFallbackLocation.MainAssembly)]
[assembly: SuppressMessage("Microsoft.Design", "CA1017:MarkAssembliesWithComVisible")]

[assembly: AssemblyVersion("1.2.12190.4000")]
[assembly: AssemblyFileVersion("1.2.12190.4000")]
[assembly: AssemblyInformationalVersion("1.2")]
