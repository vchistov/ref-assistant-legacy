//
// Copyright © 2011 Lardite.
//
// Author: Chistov Victor (vchistov@lardite.com)
//

using System;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lardite.RefAssistant.UI
{
    public sealed class ImageManager
    {
        #region Fields

        private const string ImageUriFormat = @"/{0};component/Images/{1}";
        private readonly Lazy<ImageSource> _assemblyImage;
        private static readonly string _assemblyName;

        #endregion // Fields

        #region .ctor

        public ImageManager()
        {
            _assemblyImage = new Lazy<ImageSource>(GetAssemblyImage);            
        }

        static ImageManager()
        {
            _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        }

        #endregion // .ctor

        #region Properties

        public ImageSource AssemblyImage
        {
            get
            {
                return _assemblyImage.Value;
            }
        }

        public ImageSource GetAssemblyImage()
        {
            string imageUri = string.Format(ImageUriFormat, _assemblyName, this.AssemblyImageName);
            return new BitmapImage(new Uri(imageUri, UriKind.RelativeOrAbsolute));
        }

        private string AssemblyImageName
        {
            get
            {
#if VS10
                return "Assembly_100_32bit.png";
#elif VS11
                return "Assembly_110_32bit.png";
#endif
            }
        }

        #endregion // Properties
    }
}