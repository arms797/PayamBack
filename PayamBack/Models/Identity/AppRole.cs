using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models.Identity
{
    public class AppRole : IdentityRole<int>
    {
        /// <summary>کد سیستمی یکتا برای نقش</summary>
        public int? CodeRole { get; set; }

        /// <summary>فعال/غیرفعال بودن نقش</summary>
        public bool? Vazeeyat { get; set; }

        /// <summary>آیا این نقش نیاز به امضا دارد؟</summary>
        public bool? Emza { get; set; }

        //فیلدهای ساخته شده توسط Identity

        //public int Id { get; set; }

        //public string Name { get; set; }

        //public string NormalizedName { get; set; }

        //public string ConcurrencyStamp { get; set; }
    }
}