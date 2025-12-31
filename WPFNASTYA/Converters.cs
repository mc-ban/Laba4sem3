
using CardGame.Core.Models;
using CardGame.Core.Models;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CardGame.GUI.Converters
{
    // Конвертер фракции в цвет
    public class FactionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Faction faction)
            {
                // Если параметр = "Light", возвращаем светлый оттенок
                bool isLight = parameter?.ToString() == "Light";

                return faction switch
                {
                    Faction.Humans => isLight ?
                        new SolidColorBrush(Color.FromArgb(255, 240, 230, 210)) : // Светлый бежевый
                        new SolidColorBrush(Color.FromArgb(255, 210, 180, 140)),   // Темный бежевый

                    Faction.Beasts => isLight ?
                        new SolidColorBrush(Color.FromArgb(255, 200, 150, 100)) : // Светлый коричневый
                        new SolidColorBrush(Color.FromArgb(255, 139, 69, 19)),    // Темный коричневый

                    Faction.Mythical => isLight ?
                        new SolidColorBrush(Color.FromArgb(255, 220, 180, 220)) : // Светлый фиолетовый
                        new SolidColorBrush(Color.FromArgb(255, 186, 85, 211)),   // Темный фиолетовый

                    Faction.Elements => isLight ?
                        new SolidColorBrush(Color.FromArgb(255, 160, 230, 230)) : // Светлый бирюзовый
                        new SolidColorBrush(Color.FromArgb(255, 64, 224, 208)),   // Темный бирюзовый

                    _ => isLight ? Brushes.LightGray : Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string imagePath && !string.IsNullOrWhiteSpace(imagePath))
                {
                    // Убираем лишние слеши и пробелы
                    string cleanPath = imagePath.Trim().Replace("\\", "/");

                    // Если нет папки Resources/Cards в пути, добавляем
                    if (!cleanPath.Contains("Resources/Cards"))
                    {
                        cleanPath = $"Resources/Cards/{cleanPath}";
                    }

                    // Создаем URI
                    var uri = new Uri($"pack://application:,,,/{cleanPath}");

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = uri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Image converter error: {ex.Message}");
            }

            // Возвращаем цветную заглушку
            return CreateColorPlaceholder();
        }

        private BitmapImage CreateColorPlaceholder()
        {
            // Простая цветная картинка 200x300
            var colors = new[] { Colors.DarkBlue, Colors.DarkRed, Colors.DarkGreen, Colors.Purple };
            int colorIndex = DateTime.Now.Millisecond % colors.Length;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(colors[colorIndex]), null,
                    new Rect(0, 0, 200, 300));
                dc.DrawRectangle(null, new Pen(Brushes.Gold, 2),
                    new Rect(5, 5, 190, 290));
            }

            var bmp = new RenderTargetBitmap(200, 300, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // Конвертер типа карты в видимость
    public class CardTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardType cardType)
            {
                return cardType == CardType.Creature ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер способности в иконку
    public class AbilityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string symbol)
            {
                return symbol switch
                {
                    "Провокация" => "🛡",
                    "Заряд" => "⚡",
                    "Божественный щит" => "✨",
                    "Ярость ветра" => "🌪",
                    "Яд" => "☠",
                    "Вампиризм" => "🩸",
                    "Возрождение" => "🔄",
                    "Спешка" => "🏃",
                    "Скрытность" => "👤",
                    "Урон заклинания" => "🔥",
                    "Боевой клич" => "📣",
                    "Смертельный грохот" => "💀",
                    _ => "?"
                };
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер здоровья в цвет
    public class HealthToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int health)
            {
                if (health > 10) return new SolidColorBrush(Colors.Green);
                if (health > 5) return new SolidColorBrush(Colors.Yellow);
                if (health > 2) return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Инверсный булев конвертер
    public class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    // Boolean to Visibility конвертер
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}