namespace PayamBack.Models.Captcha
{
    public class CaptchaResponse
    {
        public string CaptchaKey { get; set; } = string.Empty;
        public string CaptchaImageBase64 { get; set; } = string.Empty;
    }
}