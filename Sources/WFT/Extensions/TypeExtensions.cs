using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WFT
{
    /// <summary>
    /// Extension methods for type System.Type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Get ImageDrawing Instance From Resource Image File
        /// </summary>
        /// <param name="type">Type which is where the resource</param>
        /// <param name="resourceName">Resource FullName Path</param>
        /// <returns>Return ImageDrawing Instance of the Image Resource file</returns>
        public static ImageDrawing GetImageDrawingFromResource(this Type type, string resourceName)
        {
            return GetImageDrawingFromResource(type, resourceName, new Rect(0, 0, 16, 16));
        }

        /// <summary>
        /// Get ImageDrawing Instance From Resource Image File
        /// </summary>
        /// <param name="type">Type which is where the resource</param>
        /// <param name="resourceName">Resource FullName Path</param>
        /// <param name="rect">Image Rectangle</param>
        /// <returns>Return ImageDrawing Instance of the Image Resource file</returns>
        public static ImageDrawing GetImageDrawingFromResource(this Type type, string resourceName, Rect rect)
        {
            return new ImageDrawing(GetImageSourceFromResource(type, resourceName), rect);
        }

        /// <summary>
        /// Get ImageSource Instance From Resource Image File
        /// </summary>
        /// <param name="type">Type which is where the resource</param>
        /// <param name="resourceName">Resource FullName Path</param>
        /// <returns>Return ImageSource Instance of the Image Resource file</returns>
        public static ImageSource GetImageSourceFromResource(this Type type, string resourceName)
        {
            return BitmapFrame.Create(new Uri(
                String.Concat("pack://application:,,,/",
               type.Assembly.FullName,
                ";component/",
                resourceName
                ), UriKind.RelativeOrAbsolute));
        }
    }
}
