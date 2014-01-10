﻿using System;
using System.Collections.Generic;

namespace Lardite.RefAssistant.Model.Contracts
{
    public interface IVsProject
    {
        string Name { get; }

        string OutputAssemblyPath { get; }

        IEnumerable<VsProjectReference> References { get; }

        [Obsolete]
        Guid Kind { get; }
    }
}