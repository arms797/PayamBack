using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// تخصیص امکانات به گروه‌های کاربری
    /// </summary>
    public class RoleEmkanat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // کلید خارجی به جدول نقش (AppRole)
        [Required]
        public int RoleId { get; set; }

        // کلید خارجی به جدول امکانات (Emkanat)
        [Required]
        public int EmkanatId { get; set; }

        // وضعیت (فعال/غیرفعال)
        [Required]
        public bool Vazeeyat { get; set; } = true;

        // ======== Navigation Properties ========

        // نقش
        [ForeignKey(nameof(RoleId))]
        public AppRole? Role { get; set; }

        // امکان
        [ForeignKey(nameof(EmkanatId))]
        public Emkanat? Emkanat { get; set; }
    }
}