namespace PayamBack.DTOs.Schedule.DarsEraeh
{
    public class DarsEraehUpdateDto : DarsEraehCreateDto
    {
        // همه فیلدها ارث‌بری می‌شن
        // نکته: CodeTerm, MarkazId, ReshtehId, DarsId, Grooh معمولاً تغییر نمی‌کنن
        // ولی چون توی CreateDto هستن، اینجا هم قابل تغییرن (اگه منطق کسب‌وکار اجازه بده)
    }
}