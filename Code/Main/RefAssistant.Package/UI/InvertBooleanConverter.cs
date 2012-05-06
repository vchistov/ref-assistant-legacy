//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Windows.Data;

namespace Lardite.RefAssistant.UI
{
    /// <summary>
    /// Converter to invert boolean value in binding expression.
    /// </summary>
    sealed class InvertBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Convert value.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((targetType != typeof(bool)) && (targetType != typeof(bool?)))
            {
                throw Error.InvalidOperation(Resources.InvertBooleanConverter_InvalidOperation);
            }
            return !(bool)value;
        }

        /// <summary>
        /// Convert back value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((targetType != typeof(bool)) && (targetType != typeof(bool?)))
            {
                throw Error.InvalidOperation(Resources.InvertBooleanConverter_InvalidOperation);
            }
            return !(bool)value;
        }

        #endregion
    }
}