// PayamBack/Filters/NoPermissionAttribute.cs
namespace PayamBack.Filters
{
    /// <summary>
    /// با قرار دادن این ویژگی روی یک اکشن یا کنترلر،
    /// فیلتر PermissionFilter از بررسی مجوز برای آن صرف‌نظر می‌کند.
    /// (اما احراز هویت [Authorize] همچنان فعال است)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NoPermissionAttribute : Attribute
    {
        // این کلاس بدنه‌ای ندارد، فقط به عنوان برچسب استفاده می‌شود
    }
}