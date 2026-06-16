using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    /// <summary>
    /// تخصیص امکانات به گروه‌های کاربری
    /// </summary>
    public class RoleEmkanat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>کلید خارجی به جدول نقش (AppRole)</summary>
        public int? RoleId { get; set; }

        /// <summary>کلید خارجی به جدول امکانات (Emkanat)</summary>
        public int? EmkanatId { get; set; }

        /// <summary>وضعیت (فعال/غیرفعال)</summary>
        public bool? Vazeeyat { get; set; }

        // ======== Navigation Properties ========

        /// <summary>نقش مرتبط</summary>
        [ForeignKey(nameof(RoleId))]
        public virtual AppRole? Role { get; set; }

        /// <summary>امکان مرتبط</summary>
        [ForeignKey(nameof(EmkanatId))]
        public virtual Emkanat? Emkanat { get; set; }
    }
}