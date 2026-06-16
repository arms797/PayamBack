using Microsoft.AspNetCore.Identity;
using PayamBack.Models.Audit;
using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class AppUser : IdentityUser<int>
    {
        /// <summary>شناسه استاد در جدول Ostad</summary>
        public int? OstadId { get; set; }

        /// <summary>شناسه کارمند در جدول Karmand</summary>
        public int? KarmandId { get; set; }

        /// <summary>شناسه دانشجو در جدول Daneshjoo</summary>
        public int? DaneshjooId { get; set; }

        /// <summary>وضعیت فعال/غیرفعال کاربر</summary>
        public bool? Vazeeyat { get; set; }

        /// <summary>وضعیت موقت کاربر (مانند مسدودیت موقت)</summary>
        public bool? VazeeyatMovaghat { get; set; }

        // ======== Navigation Properties ========

        /// <summary>اطلاعات استاد مرتبط</summary>
        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        /// <summary>اطلاعات کارمند مرتبط</summary>
        [ForeignKey(nameof(KarmandId))]
        public virtual Karmand? Karmand { get; set; }

        /// <summary>اطلاعات دانشجو مرتبط</summary>
        [ForeignKey(nameof(DaneshjooId))]
        public virtual Daneshjoo? Daneshjoo { get; set; }

        /// <summary>سوابق فعالیت‌های کاربر</summary>
        public virtual ICollection<Sabeghe>? Sabeghes { get; set; }
    }
}