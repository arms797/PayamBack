using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class KarbarReshteh
    {
        [Key]
        public int Id { get; set; }
        public int AppUserRoleId { get; set; }
        public int? ReshtehId { get; set; }
        public int? ServiciId { get; set; }
        public int Level { get; set; } = 4;
        public int MarkazId { get; set; }

        [ForeignKey(nameof(AppUserRoleId))]
        public AppUserRole AppUserRole { get; set; }
        [ForeignKey(nameof(ReshtehId))]
        public Reshteh Reshteh { get; set; }
        [ForeignKey(nameof(ServiciId))]
        public Servici Servici { get; set; }
        [ForeignKey(nameof(MarkazId))]
        public Markaz Markaz { get; set; }


    }
}
