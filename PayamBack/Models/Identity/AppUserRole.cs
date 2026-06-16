using Microsoft.AspNetCore.Identity;
using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class AppUserRole : IdentityUserRole<int>
    {
        /// <summary>کلید خارجی به جدول مرکز (Markaz)</summary>
        public int? MarkazId { get; set; }

        /// <summary>نقش پیش‌فرض این کاربر در مرکز مربوطه</summary>
        public bool? RolePishFarz { get; set; }

        /// <summary>دسترسی به اطلاعات مرکز</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }
    }
}