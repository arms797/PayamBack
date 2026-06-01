using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class Sabeghe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }                   // not null

        [Required]
        [MaxLength(50)]
        public string IpSystem { get; set; } = "";    // not null (string.Empty ذخیره می‌شود)

        [Required]
        [MaxLength(200)]
        public string Dastgah { get; set; } = "";

        [Required]
        [MaxLength(200)]
        public string Moroorgar { get; set; } = "";

        [Required]
        public DateTime? ZamanLogin { get; set; }      // not null (در صورت نبودن مقدار باید DateTime.MinValue یا یک تاریخ قراردادی بفرستید)

        [Required]
        [MaxLength(100)]
        public string Table { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string User { get; set; }              // not null (حتماً باید مقدار داشته باشد)

        [Required]
        [MaxLength(100)]
        public string IdRecordTagirDahande { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string RoozHafte { get; set; } = "";

        [Required]
        public DateTime? ZamanTagir { get; set; }

        [Required]
        [MaxLength(1000)]
        public string TozihTagirat { get; set; } = "";

        [Required]
        public DateTime? ZamanLogOut { get; set; }     // not null (اگر هنوز لاگ‌اوت نشده، یک تاریخ پیش‌فرض بگذارید)
    }
}
