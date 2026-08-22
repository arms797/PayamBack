using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    public class BarnamehHaftegiOstad
    {
        [Key]
        public int Id { get; set; }

        /// <summary>کلید خارجی به جدول استاد</summary>
        public int OstadId { get; set; }

        /// <summary>کد ترم</summary>
        [MaxLength(50)]
        public string CodeTerm { get; set; }    
        //0=ثبت اولیه
        //1=تایید
        //2=رد
        public int? NararElmi { get; set; }
        //تاریخ انجام
        public DateTime? TarikhElmi {  get; set; }

        //آیدی مدیر گروه
        public int? UserIdModirGrooh { get; set; }
        //نقش و مرکز مدیرگروه
        public string? RoleMarkazModirGrooh { get; set; }
        //0=معادل بدون نظر
        //1=تایید
        //2=رد
        public int? NazarModirGrooh { get; set; }
        //تاریخ انجام
        public DateTime? TarikhModirGrooh { get; set; }

        //آیدی معاون
        public int? UserIdMoaven { get; set; }
        //نقش و مرکز معاون
        public string? RoleMarkazMoaven { get; set; }
        //0=معادل بدون نظر
        //1=تایید
        //2=رد
        public int? NazarMoaven { get; set; }
        //تاریخ انجام
        public DateTime? TarikhMoaven { get; set; }


        // ======== Navigation Properties ========

        /// <summary>استاد مرتبط</summary>
        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        /// <summary>ترم مرتبط</summary>
        [ForeignKey(nameof(CodeTerm))]
        public virtual Term? Term { get; set; }
        [ForeignKey(nameof(UserIdModirGrooh))]
        public virtual AppUser? AppUserModirGrooh { get; set; }
        [ForeignKey(nameof(UserIdMoaven))]
        public virtual AppUser? AppUserMoaven { get; set; }

        // ======== Collection ========
        public virtual ICollection<BarnamehHaftegiOstad1> BarnamehHaftegiOstad1s { get; set; }
    }
}