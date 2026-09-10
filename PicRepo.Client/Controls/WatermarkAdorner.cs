using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PicRepo.Client.Controls;

public class WatermarkAdorner : Adorner
{
    private readonly string _watermarkText;
    private readonly Brush _foreground;
    private readonly FontFamily _fontFamily;
    private readonly double _fontSize;

    public WatermarkAdorner(UIElement adornedElement, string watermarkText)
        : base(adornedElement)
    {
        _watermarkText = watermarkText;

        // 从 TextBox 继承样式属性，使水印看起来更自然
        if (adornedElement is TextBox textBox)
        {
            _foreground = new SolidColorBrush(Colors.Gray);
            _fontFamily = textBox.FontFamily;
            _fontSize = textBox.FontSize;
        }
        else
        {
            _foreground = new SolidColorBrush(Colors.Gray);
            _fontFamily = new FontFamily("Segoe UI");
            _fontSize = 12;
        }

        // 让 Adorner 可以接收鼠标事件，避免干扰文本框操作
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (AdornedElement is TextBox textBox)
        {
            // 获取文本框的当前文本布局信息
            var formattedText = new FormattedText(
                _watermarkText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(_fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                _fontSize,
                _foreground,
                VisualTreeHelper.GetDpi(textBox).PixelsPerDip);

            // 计算绘制位置（根据文本框的 Padding 缩进）
            var padding = textBox.Padding;
            var x = padding.Left + 2; // 再加2像素确保美观
            var y = padding.Top + 2;

            // 如果文本框有垂直内容对齐，调整绘制位置
            if (textBox.VerticalContentAlignment == VerticalAlignment.Center)
            {
                var textHeight = formattedText.Height;
                var boxHeight = textBox.ActualHeight - padding.Top - padding.Bottom;
                y = (boxHeight - textHeight) / 2 + padding.Top;
            }

            // 绘制水印文字
            drawingContext.DrawText(formattedText, new Point(x, y));
        }
    }
}