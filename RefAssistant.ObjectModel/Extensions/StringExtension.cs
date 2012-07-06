//
// Copyright © 2011-2012 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;

namespace Lardite.RefAssistant.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="string"/> class.
    /// </summary>
    public static class StringExtension
    {
        public static string TrimEnd(this string str)
        {
            return str.TrimEnd((Environment.NewLine + "\t ").ToCharArray());
        }
    }
}
