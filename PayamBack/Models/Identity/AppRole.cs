using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models.Identity
{
    public class AppRole : IdentityRole<int>
    {
        /// <summary>کد لول نقش (هر چه کمتر، سطح بالاتر)</summary>
        public int? CodeRole { get; set; }

        /// <summary>فعال/غیرفعال بودن نقش</summary>
        public bool? Vazeeyat { get; set; }

        /// <summary>آیا این نقش نیاز به امضا دارد؟</summary>
        public bool? Emza { get; set; }

        /// <summary>آیا این نقش یک نقش ادمین است؟</summary>
        public bool? IsAdmin { get; set; } = false;

        /// <summary>
        /// آیا این نقش در هر مرکز فقط به یک کاربر قابل تخصیص است؟
        /// اگر true باشد: فقط یک کاربر در هر مرکز می‌تواند این نقش را داشته باشد.
        /// اگر false باشد: چند کاربر در یک مرکز می‌توانند این نقش را داشته باشند.
        /// </summary>
        public bool? IsUniquePerMarkaz { get; set; } = false;

        //فیلدهای ساخته شده توسط Identity
        //public int Id { get; set; }
        //public string Name { get; set; }
        //public string NormalizedName { get; set; }
        //public string ConcurrencyStamp { get; set; }

        public virtual ICollection<AppUserRole>? AppUserRoles { get; set; }

    }
}