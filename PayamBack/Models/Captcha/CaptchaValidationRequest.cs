namespace PayamBack.Models.Captcha
{
    public class CaptchaValidationRequest
    {
        public string CaptchaKey { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
    }
}