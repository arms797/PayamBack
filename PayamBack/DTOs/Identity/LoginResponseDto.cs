namespace PayamBack.DTOs.Identity
{
    /// <summary>
    /// DTO پاسخ ورود یا تمدید توکن
    /// </summary>
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;      // توکن اصلی JWT
        public string RefreshToken { get; set; } = string.Empty;     // برای تمدید توکن
        public string Username { get; set; } = string.Empty;         // نام کاربری
        public string Email { get; set; } = string.Empty;            // ایمیل
        public string FirstName { get; set; } = string.Empty;        // ← نام
        public string LastName { get; set; } = string.Empty;         // ← نام خانوادگی
        public int? CurrentRoleId { get; set; }                      // شناسه نقش فعال فعلی
        public string CurrentRoleName { get; set; } = string.Empty;  // نام نقش فعال فعلی
        public int? MarkazId { get; set; }                           //مرکز نقش فعال
        public List<RoleDto> Roles { get; set; } = new();           // لیست همه نقش‌ها
        public List<MenuDto> Menus { get; set; } = new();           // منوهای قابل نمایش
        public List<string> Permissions { get; set; } = new();      //لیست مجوزهای نقش فعال
        public int ExpiresIn { get; set; }                          // مدت اعتبار توکن (دقیقه)
    }

    /// <summary>
    /// DTO برای نمایش نقش‌های کاربر در کامبوباکس
    /// </summary>
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public int MarkazId { get; set; }
        public int CodeRole { get; set; } = 4;
        public bool IsAdmin {  get; set; }=false;
        //public bool IsUniquePerMarkazId {  get; set; } = false;
    }
}