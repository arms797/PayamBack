using Microsoft.Extensions.Caching.Memory;
using PayamBack.Models.Captcha;
using PayamBack.Services.Interfaces;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PayamBack.Services.Implementations
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        private static readonly Random _random = new Random();
        private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public CaptchaService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public CaptchaResponse GenerateCaptcha()
        {
            var captchaText = GenerateRandomText(5);
            var captchaKey = Guid.NewGuid().ToString("N");
            var imageBase64 = GenerateCaptchaImage(captchaText);

            _cache.Set(captchaKey, captchaText, TimeSpan.FromMinutes(5));

            return new CaptchaResponse
            {
                CaptchaKey = captchaKey,
                CaptchaImageBase64 = imageBase64
            };
        }

        public bool ValidateCaptcha(string captchaKey, string userAnswer)
        {
            if (string.IsNullOrEmpty(captchaKey) || string.IsNullOrEmpty(userAnswer))
                return false;

            if (!_cache.TryGetValue(captchaKey, out string? storedText))
                return false;

            var normalizedUserAnswer = new string(userAnswer.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            var normalizedStoredText = storedText?.ToUpper();

            return normalizedUserAnswer == normalizedStoredText;
        }

        public void RemoveCaptcha(string captchaKey)
        {
            if (!string.IsNullOrEmpty(captchaKey))
                _cache.Remove(captchaKey);
        }

        private string GenerateRandomText(int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = Characters[_random.Next(Characters.Length)];
            }
            return new string(chars);
        }

        private string GenerateCaptchaImage(string text)
        {
            int width = 220;
            int height = 80;

            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);

            // تنظیم کیفیت بالا
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // پس‌زمینه سفید
            graphics.Clear(Color.White);

            // ============================================================
            // رسم حروف با چرخش و کجی
            // ============================================================
            var font = new Font("Arial", 24, FontStyle.Bold);

            float x = 10;
            float y = 15;

            foreach (char c in text)
            {
                var color = GetRandomColor();
                var rotation = _random.Next(-30, 30);

                // ذخیره وضعیت فعلی
                var state = graphics.Save();

                // چرخش
                graphics.TranslateTransform(x + 10, y + 20);
                graphics.RotateTransform(rotation);

                // رسم حرف
                graphics.DrawString(
                    c.ToString(),
                    font,
                    new SolidBrush(color),
                    -5,
                    -10 + _random.Next(-5, 5));

                // بازگشت به حالت اولیه
                graphics.Restore(state);

                x += 28 + _random.Next(5, 15);
            }

            // ============================================================
            // اضافه کردن نویز (خطوط و نقاط)
            // ============================================================

            // خطوط نویز
            for (int i = 0; i < 5; i++)
            {
                var pen = new Pen(GetRandomColor(), 1);
                graphics.DrawLine(
                    pen,
                    _random.Next(0, width),
                    _random.Next(0, height),
                    _random.Next(0, width),
                    _random.Next(0, height));
            }

            // نقاط نویز
            for (int i = 0; i < 50; i++)
            {
                bitmap.SetPixel(
                    _random.Next(0, width),
                    _random.Next(0, height),
                    GetRandomColor());
            }

            // ============================================================
            // تبدیل به Base64
            // ============================================================
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            var imageBytes = ms.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        private Color GetRandomColor()
        {
            var colors = new[]
            {
                Color.Blue, Color.Red, Color.Green, Color.Purple,
                Color.Orange, Color.DarkBlue, Color.DarkRed,
                Color.DarkGreen, Color.DarkGoldenrod, Color.DeepPink,
                Color.Brown, Color.Cyan, Color.Magenta, Color.Olive,
                Color.SteelBlue
            };
            return colors[_random.Next(colors.Length)];
        }
    }
}