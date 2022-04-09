using System.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
namespace UWPDrawingCanvas
{
    class CalligraphicPen : InkToolbarCustomPen
    {
        protected override InkDrawingAttributes CreateInkDrawingAttributesCore(Brush brush, double strokeWidth)
        {
            InkDrawingAttributes inkDrawingAttributes = new InkDrawingAttributes
            {
              PenTip = PenTipShape.Circle,
              Size = new Windows.Foundation.Size(strokeWidth, strokeWidth * 20)
            };
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null) inkDrawingAttributes.Color = solidColorBrush.Color;
            else inkDrawingAttributes.Color = Colors.Black;
            Matrix3x2 matrix = Matrix3x2.CreateRotation(45);
            inkDrawingAttributes.PenTipTransform = matrix;
            return inkDrawingAttributes;
        }
    }
}
