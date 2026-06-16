using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
{
    /// <summary>
    /// واحد / مرکز
    /// </summary>
    public class Markaz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>کد استان</summary>
        [MaxLength(50)]
        public string? CodeOstan { get; set; }

        /// <summary>نام استان</summary>
        [MaxLength(200)]
        public string? NaamOstan { get; set; }

        /// <summary>کد واحد یا مرکز</summary>
        [MaxLength(50)]
        public string? CodeMarkaz { get; set; }

        /// <summary>نام واحد یا مرکز</summary>
        [MaxLength(200)]
        public string? NaamMarkaz { get; set; }

        /// <summary>واحد / مرکز</summary>
        [MaxLength(100)]
        public string? VahedMarkaz { get; set; }

        /// <summary>ناحیه</summary>
        [MaxLength(50)]
        public string? Nahiyeh { get; set; }

        /// <summary>محل واحد یا مرکز</summary>
        [MaxLength(200)]
        public string? MahalMarkaz { get; set; }

        /// <summary>آدرس</summary>
        [MaxLength(500)]
        public string? Adres { get; set; }

        /// <summary>کد پستی</summary>
        [MaxLength(20)]
        public string? CodePosti { get; set; }

        /// <summary>آدرس سایت</summary>
        [MaxLength(200)]
        public string? WebSite { get; set; }

        /// <summary>تلفن</summary>
        [MaxLength(20)]
        public string? Telefon { get; set; }

        /// <summary>وضعیت (فعال/غیرفعال)</summary>
        public bool? Vazeeyat { get; set; }

        // ======== Navigation Properties (ICollection) ========

        /// <summary>نقش‌های کاربری مرتبط با این مرکز</summary>
        public virtual ICollection<AppUserRole>? AppUserRoles { get; set; }

        /// <summary>اساتید مرتبط با این مرکز</summary>
        public virtual ICollection<Ostad>? Ostads { get; set; }

        /// <summary>دانشجویان مرتبط با این مرکز</summary>
        public virtual ICollection<Daneshjoo>? Daneshjoos { get; set; }

        /// <summary>کارمندان مرتبط با این مرکز</summary>
        public virtual ICollection<Karmand>? Karmands { get; set; }

        /// <summary>برنامه هفتگی اساتید مرتبط با این مرکز</summary>
        public virtual ICollection<BarnamehHaftegiOstad>? BarnamehHaftegiOstads { get; set; }

        /// <summary>برنامه ترمی اساتید مرتبط با این مرکز</summary>
        public virtual ICollection<BarnamehTermiOstad>? BarnamehTermiOstads { get; set; }
    }
}