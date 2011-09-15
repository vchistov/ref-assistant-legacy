//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Provides a way of converting PublicKeyToken value from byte array to string and back.
    /// </summary>
    class PublicKeyTokenConverter : TypeConverter
    {
        #region Public methods

        /// <summary>
        /// Converts the given public key token (byte array) to the string object.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public string ConvertFrom(byte[] value)
        {
            return ConvertFrom(value as object) as string;
        }

        /// <summary>
        /// Converts the given value string of public key token to the byte array.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public byte[] ConvertTo(string value)
        {
            return ConvertTo(value as object, typeof(byte[])) as byte[];
        }

        #endregion // Public methods

        #region TypeConverter overrides

        /// <summary>
        /// Returns whether this converter can convert an object of the byte[] type to the string type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(byte[]))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given public key token (byte array) to the string type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(byte[]))
            {
                var publicKeyTokenArray = (byte[])value;
                string publicKeyToken = string.Empty;

                Array.ForEach(publicKeyTokenArray, t => { publicKeyToken += t.ToString("x2", culture); });
                return publicKeyToken;
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Returns whether this converter can convert the string object to the byte array, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(byte[]))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }
               
        /// <summary>
        /// Converts the given value string object to the byte array, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">>The <see cref="Object"/> to convert.</param>
        /// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(byte[]) && value.GetType() == typeof(string))
            {
                var result = new List<byte>();
                var publicKeyTokenString = (string)value;

                for (int i = 0; i < publicKeyTokenString.Length; i += 2)
                {
                    var str = publicKeyTokenString.Substring(i, 2);
                    result.Add(byte.Parse(str, NumberStyles.AllowHexSpecifier, culture));
                }

                return result.ToArray();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }       

        #endregion // TypeConverter overrides
    }
}
