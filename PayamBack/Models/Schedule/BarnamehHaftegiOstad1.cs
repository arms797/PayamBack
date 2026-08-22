using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    public class BarnamehHaftegiOstad1
    {
        [Key]
        public int Id { get; set; } 
        
        //کلید خارجی آیدی برنامه هفتگی استاد
        public int BarnamehHaftegiOstadId {  get; set; }

        //شهر انتخابی فعالیت حضوری
        public int? MarkazId { get; set; }

        /// <summary>روز هفته (شنبه، یکشنبه، ...)</summary>
        [MaxLength(50)]
        public string? RoozeHafteh { get; set; }

        /// <summary>ساعت A (کد وضعیت - 0 یعنی خالی/بدون وضعیت)</summary>
        public int? A { get; set; }
        //مرکز ساعت A
        public int? MarkazIdA { get; set; }

        /// <summary>ساعت B</summary>
        public int? B { get; set; }
        //مرکز ساعت B
        public int? MarkazIdB { get; set; }

        /// <summary>ساعت C</summary>
        public int? C { get; set; }
        //مرکز ساعت C
        public int? MarkazIdC { get; set; }

        /// <summary>ساعت D</summary>
        public int? D { get; set; }
        //مرکز ساعت D
        public int? MarkazIdD { get; set; }

        /// <summary>ساعت E</summary>
        public int? E { get; set; }
        //مرکز ساعت E
        public int? MarkazIdE { get; set; }

        /// <summary>ساعت F</summary>
        public int? F { get; set; }
        //مرکز ساعت F
        public int? MarkazIdF { get; set; }

        /// <summary>ساعت G</summary>
        public int? G { get; set; }
        //مرکز ساعت G
        public int? MarkazIdG { get; set; }

        /// <summary>ساعت H</summary>
        public int? H { get; set; }
        //مرکز ساعت H
        public int? MarkazIdH { get; set; }

        /// <summary>جزئیات بیشتر</summary>
        public bool? Jozeiat { get; set; }        

        // ======== Navigation Properties ========

        [ForeignKey(nameof(BarnamehHaftegiOstadId))]
        public virtual BarnamehHaftegiOstad? BarnamehHaftegiOstad { get; set; }
        /*[ForeignKey (nameof(MarkazId))]
        public virtual Markaz? Markaz {  get; set; }

        [ForeignKey (nameof(MarkazIdA))]
        public virtual Markaz? MarkazA { get; set; }

        [ForeignKey(nameof(MarkazIdB))]
        public virtual Markaz? MarkazB { get; set; }

        [ForeignKey(nameof(MarkazIdC))]
        public virtual Markaz? MarkazC { get; set; }

        [ForeignKey(nameof(MarkazIdD))]
        public virtual Markaz? MarkazD { get; set; }

        [ForeignKey(nameof(MarkazIdE))]
        public virtual Markaz? MarkazE { get; set; }

        [ForeignKey(nameof(MarkazIdF))]
        public virtual Markaz? MarkazF { get; set; }

        [ForeignKey(nameof(MarkazIdG))]
        public virtual Markaz? MarkazG { get; set; }

        [ForeignKey(nameof(MarkazIdH))]
        public virtual Markaz? MarkazH { get; set; }

        [ForeignKey(nameof(A))]
        public virtual Faaliat? FaaliatA { get; set; }

        [ForeignKey(nameof(B))]
        public virtual Faaliat? FaaliatB { get; set; }

        [ForeignKey(nameof(C))]
        public virtual Faaliat? FaaliatC { get; set; }

        [ForeignKey(nameof(D))]
        public virtual Faaliat? FaaliatD { get; set; }

        [ForeignKey(nameof(E))]
        public virtual Faaliat? FaaliatE { get; set; }

        [ForeignKey(nameof(F))]
        public virtual Faaliat? FaaliatF { get; set; }

        [ForeignKey(nameof(G))]
        public virtual Faaliat? FaaliatG { get; set; }

        [ForeignKey(nameof(H))]
        public virtual Faaliat? FaaliatH { get; set; }*/
    }
}