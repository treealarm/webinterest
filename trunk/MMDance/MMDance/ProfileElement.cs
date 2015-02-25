using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;

namespace MMDance
{
    public class ProfileElement
    {
        public string FileName { get; set; }
        public int Length { get; set; }
        public BitmapImage Image
        {
            get
            {
                if (!File.Exists(FileName))
                {
                    return null;
                }

                BitmapImage image = new BitmapImage();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.BeginInit();
                image.UriSource = new Uri(FileName, UriKind.RelativeOrAbsolute); 
                image.EndInit();
                return image;
            }
        }

    }
}
