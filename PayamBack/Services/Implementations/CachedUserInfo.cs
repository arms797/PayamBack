// PayamBack/Services/Implementations/CachedUserInfo.cs
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Services.Implementations
{
    /// <summary>
    /// اطلاعات کاربر که در کش ذخیره می‌شود
    /// </summary>
    public class CachedUserInfo
    {
        public AppUser User { get; set; } = null!;
        public AppRole? Role { get; set; }
        public Markaz? Markaz { get; set; }
        public int CodeRole { get; set; }
    }
}