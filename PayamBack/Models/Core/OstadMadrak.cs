using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
{
    /// <summary>
    /// رشته های تحصیلی استاد
    /// </summary>
    public class OstadMadrak
    {
        [Key]
        public int Id { get; set; }

        /// <summary> کلید خارجی به جدول استاد</summary>
        public int? OstadId { get; set; }

        /// <summary> گروه آموزشی استاد</summary>
        public int? GrooheAmoozeshiId { get; set; }

        /// <summary>عنوان رشته تحصیلی استاد</summary>
        [MaxLength(100)]
        public string? Reshteh { get; set; }

        /// <summary> گرایش  </summary>
        [MaxLength(100)]
        public string? Grayesh { get; set; }

        /// <summary>  مقطع تحصیلی استاد</summary>
        public int? Maghta { get; set; }

        /// <summary>   رشته پیش فرض استاد</summary>
        public bool? PishFarz { get; set; }

        /// <summary>محل اخذ مدرک </summary>
        [MaxLength(100)]
        public string? MahalAkhz { get; set; }

        /// <summary> تصویر مدرک </summary>
        [MaxLength(250)]
        public string? TasvirMadrak { get; set; }

        // ============================================================
        // 🔥 فیلدهای ایجاد
        // ============================================================

        /// <summary>شناسه کاربر ایجاد کننده مدرک</summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>نام نقش و مرکز کاربر ایجاد کننده (برای نمایش)</summary>
        [MaxLength(200)]
        public string? CreatedByRoleInfo { get; set; }

        /// <summary>تاریخ ایجاد مدرک</summary>
        public DateTime? CreatedAt { get; set; }

        // ============================================================
        // 🔥 فیلدهای تایید
        // ============================================================

        /// <summary>وضعیت تایید مدرک (true = تایید شده)</summary>
        public bool? IsApproved { get; set; }

        /// <summary>شناسه کاربر تایید کننده</summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>نام نقش و مرکز کاربر تایید کننده (برای نمایش)</summary>
        [MaxLength(200)]
        public string? ApprovedByRoleInfo { get; set; }

        /// <summary>تاریخ تایید مدرک</summary>
        public DateTime? ApprovedAt { get; set; }

        // ============================================================
        // 🔥 Navigation Properties
        // ============================================================

        /// <summary>گروه آموزشی استاد</summary>
        [ForeignKey(nameof(GrooheAmoozeshiId))]
        public virtual GrooheAmoozeshi? GrooheAmoozeshi { get; set; }

        /// <summary>استاد</summary>
        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        /// <summary>کاربر ایجاد کننده</summary>
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual AppUser? CreatedByUser { get; set; }

        /// <summary>کاربر تایید کننده</summary>
        [ForeignKey(nameof(ApprovedByUserId))]
        public virtual AppUser? ApprovedByUser { get; set; }
    }
}