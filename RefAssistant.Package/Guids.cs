//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Guid list.
    /// </summary>
    public static class GuidList
    {
        public const string guidRefAssistant100PkgString = "CA8E8309-7ED1-4F8C-A768-7A8CAE5D165E";
        public const string guidRefAssistant110PkgString = "F9E800C2-C25C-4DEA-8C14-2EF7A18A8A51";
        public const string guidRefAssistantCmdSetString = "820B0D5D-901D-44D0-B956-CA55F308BDC4";

        public static readonly Guid guidRefAssistantCmdSet = new Guid(guidRefAssistantCmdSetString);
    }

    /// <summary>
    /// Kinds of projects which are unsupported
    /// </summary>
    public static class ProjectKinds
    {
        public static readonly Guid CSharp = new Guid("fae04ec0-301f-11d3-bf4b-00c04f79efbc");
        public static readonly Guid VisualBasic = new Guid("f184b08f-c81c-45f6-a57f-5abd9991f28f");
        public static readonly Guid VisualCppCli = new Guid("8bc9ceb8-8b4a-11d0-8d11-00a0c91bc942");
        public static readonly Guid FSharp = new Guid("f2a71f9b-5d33-465a-a702-920d77279786");

#warning TODO: possible useless
        public static readonly Guid Modeling = new Guid("f088123c-0e9e-452a-89e6-6ba2f21d5cac");
        public static readonly Guid Database = new Guid("c8d11400-126e-41cd-887f-60bd40844f9e");
    }
}