namespace PayamBack.DTOs.Edu.Dars
{
    public class ManbaDarsSimpleDto
    {
        public int Id { get; set; }                    // ← آیدی منبع (برای دیباگ)
        public int DarsId { get; set; }                // 🔥 آیدی درس
        public string? ShomareManba { get; set; }      // 🔥 شماره منبع
        public string? Onvan { get; set; }             // 🔥 عنوان
        public string? CodePeyvast { get; set; }       // 🔥 کد پیوست
        public int ManbaIndex { get; set; }            // 🔥 شماره ترتیب (1, 2, 3, ...)
    }
}