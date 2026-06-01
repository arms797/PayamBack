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

        /// <summary>کد گروه کاربری</summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>کد امکانات تخصیص داده شده</summary>
        [Required]
        public int CodeEmkanatTakhsisi { get; set; }

        /// <summary>وضعیت (فعال/غیرفعال)</summary>
        [Required]
        public bool Vazeeyat { get; set; }
    }
}
