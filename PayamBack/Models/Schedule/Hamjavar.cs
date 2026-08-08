using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    public class Hamjavar
    {
        [Key]
        public int Id { get; set; }

        // ============================================================
        // اطلاعات پایه
        // ============================================================
        public int OstadId { get; set; }            // کلید خارجی به Ostad
        public string? TermCode { get; set; }       //  ترم
        public int? UserIdSabtKonandeh { get; set; }    //Id کاربر ایجاد کننده
        public string? RoleMarkazSabtKonandeh {  get; set; }// نقش و مرکز کاربر ایجاد کننده

        // اطلاعات تدریس
        public decimal? VahedMovazaf {  get; set; }     // تعداد واحد معادل موظف 
        public decimal? TedadVahedMahalKhedmat { get; set; }    // تعداد واحد در محل خدمت
        public decimal? TedadVahedHamjavar { get; set; }        // فعالیت حضوری در مراکز دیگر
        public decimal? TedadVahedMajazi { get; set; }          // فعالیت مجازی در مراکز دیگر
        public string? Dalil { get; set; }                 // دلایل تقاضا
        public string? ShahrZendegi { get; set; }          // شهر محل سکونت

        // ============================================================
        // مرحله 1: عضو علمی
        // ============================================================
        public string? UploadElmi { get; set; }            // بارگذاری مستندات
        public int? AmaliatElmi { get; set; }              // عملیات (مثلاً 1=تایید، 0=رد)
        public string? NazarElmi { get; set; }             // نظر
        public DateTime? TarikhErsalElmi { get; set; }     // تاریخ ارسال به مرحله بعد

        // ============================================================
        // مرحله 2: رئیس مرکز
        // ============================================================
        public DateTime? TarikhDaryaftRaeis { get; set; }  // تاریخ دریافت
        public string? TozihatRaeis { get; set; }          // توضیحات
        public string? UploadRaeis { get; set; }           // بارگذاری مستندات
        public int? AmaliatRaeis { get; set; }             // عملیات
        public string? NazarRaeis { get; set; }            // نظر
        public DateTime? TarikhErsalRaeis { get; set; }    // تاریخ ارسال به مرحله بعد
        public int? UserIdRaeis { get; set; }    //Id رییس تایید کننده
        public string? RoleMarkazRaeis { get; set; }// نقش و مرکز کاربر تایید کننده در مرکز

        // ============================================================
        // مرحله 3: مدیر خدمات آموزشی استان
        // ============================================================
        public DateTime? TarikhDaryaftKhadamat { get; set; }   // تاریخ دریافت
        public string? TozihatKhadamat { get; set; }           // توضیحات
        public string? UploadKhadamat { get; set; }            // بارگذاری مستندات
        public int? AmaliatKhadamat { get; set; }              // عملیات
        public string? NazarKhadamat { get; set; }             // نظر
        public DateTime? TarikhErsalKhadamat { get; set; }     // تاریخ ارسال به مرحله بعد
        public int? UserIdKhadamatOstan { get; set; }    //Id خدمات آموزشی استان تایید کننده
        public string? RoleMarkazKhadamatOstan { get; set; }// نقش و مرکز کاربر تایید کننده خدمات اموزشی

        // ============================================================
        // مرحله 4: معاونت آموزشی استان
        // ============================================================
        public DateTime? TarikhDaryaftMoaven { get; set; }     // تاریخ دریافت
        public string? TozihatMoaven { get; set; }             // توضیحات
        public string? UploadMoaven { get; set; }              // بارگذاری مستندات
        public int? AmaliatMoaven { get; set; }                // عملیات
        public string? NazarMoaven { get; set; }               // نظر
        public DateTime? TarikhErsalMoaven { get; set; }       // تاریخ نظر نهایی
        public int? UserIdApproved { get; set; }    //Id معاون آموزشی استان تایید کننده
        public string? RoleMarkazApproved { get; set; }// نقش و مرکز کاربر تایید کننده معاون استان
        // ============================================================
        // وضعیت نهایی
        // ============================================================
        public string? AKharinBarrasi { get; set; }      // آخرین مرحله بررسی شده
        public string? AkharinTaghaza { get; set; }     // آخرین وضعیت تقاضا

        // ============================================================
        // Navigation Properties
        // ============================================================
        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        [ForeignKey(nameof(TermCode))]
        public virtual Term? Term { get; set; }

        public virtual Collection<Hamjavar1> Hamjavar1s {  get; set; }
    }
}
