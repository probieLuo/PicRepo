using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PicRepo.Client.Controls;

public static class WatermarkHelper
{
    // 注册附加属性 Watermark
    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(string),
            typeof(WatermarkHelper),
            new PropertyMetadata(null, OnWatermarkChanged));

    // Getter 和 Setter
    public static string GetWatermark(DependencyObject obj)
    {
        return (string)obj.GetValue(WatermarkProperty);
    }

    public static void SetWatermark(DependencyObject obj, string value)
    {
        obj.SetValue(WatermarkProperty, value);
    }

    private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            // 移除旧的 Adorner
            RemoveWatermarkAdorner(textBox);

            var watermark = e.NewValue as string;
            if (!string.IsNullOrEmpty(watermark))
            {
                // 添加新的 Adorner
                AddWatermarkAdorner(textBox, watermark);

                // 订阅文本变化事件
                textBox.TextChanged += OnTextChanged;
                textBox.Loaded += OnLoaded;

                // 窗口卸载时清理资源
                if (textBox.IsLoaded)
                {
                    UpdateAdornerVisibility(textBox);
                }
            }
            else
            {
                // 清除事件订阅
                textBox.TextChanged -= OnTextChanged;
                textBox.Loaded -= OnLoaded;
            }
        }
    }

    private static void AddWatermarkAdorner(TextBox textBox, string watermark)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer != null)
        {
            // 如果已存在 WatermarkAdorner 则不重复添加
            var adorners = adornerLayer.GetAdorners(textBox);
            if (adorners != null)
            {
                foreach (var item in adorners)
                {
                    if (item is WatermarkAdorner)
                    {
                        return;
                    }
                }
            }

            var adorner = new WatermarkAdorner(textBox, watermark);
            adornerLayer.Add(adorner);

            // 存储 Adorner 引用以便后续操作
            textBox.Tag = adorner;
        }
    }

    private static void RemoveWatermarkAdorner(TextBox textBox)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer != null)
        {
            // 从 Tag 中获取 Adorner 引用
            if (textBox.Tag is WatermarkAdorner adorner)
            {
                adornerLayer.Remove(adorner);
                textBox.Tag = null;
            }

            // 备用方案：遍历移除所有 WatermarkAdorner
            var adorners = adornerLayer.GetAdorners(textBox);
            if (adorners != null)
            {
                foreach (var item in adorners)
                {
                    if (item is WatermarkAdorner)
                    {
                        adornerLayer.Remove(item);
                    }
                }
            }
        }
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateAdornerVisibility(textBox);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // 尝试确保在加载后添加 Adorner（当初次获取 AdornerLayer 为 null 时生效）
            var watermark = GetWatermark(textBox);
            if (!string.IsNullOrEmpty(watermark))
            {
                AddWatermarkAdorner(textBox, watermark);
            }
            UpdateAdornerVisibility(textBox);
        }
    }

    private static void UpdateAdornerVisibility(TextBox textBox)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer == null) return;

        var adorners = adornerLayer.GetAdorners(textBox);
        if (adorners == null) return;

        foreach (var adorner in adorners)
        {
            if (adorner is WatermarkAdorner)
            {
                // 当文本框有内容时隐藏 Adorner，为空时显示
                adorner.Visibility = string.IsNullOrEmpty(textBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}