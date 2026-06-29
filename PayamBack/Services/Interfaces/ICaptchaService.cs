using PayamBack.Models.Captcha;

namespace PayamBack.Services.Interfaces
{
    public interface ICaptchaService
    {
        CaptchaResponse GenerateCaptcha();
        bool ValidateCaptcha(string captchaKey, string userAnswer);
        void RemoveCaptcha(string captchaKey);
    }
}