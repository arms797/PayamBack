using Microsoft.AspNetCore.Identity;
using PayamBack.Models.Audit;
using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class AppUser:IdentityUser<int>
    {
        // شناسه کاربر در جداول تخصصی (به جای CodeNoeUser)
        public int? OstadId { get; set; }
        public int? KarmandId { get; set; }
        public int? DaneshjooId { get; set; }

        public bool Vazeeyat { get; set; } = true;
        public bool VazeeyatMovaghat { get; set; } = true;

        [ForeignKey(nameof(OstadId))]
        public Ostad? Ostad { get; set; }

        [ForeignKey(nameof(KarmandId))]
        public Karmand? Karmand { get; set; }

        [ForeignKey(nameof(DaneshjooId))]
        public Daneshjoo? Daneshjoo { get; set; }

        // Navigation Properties (ICollection)
        public ICollection<Sabeghe>? Sabeghes { get; set; }
    }
}
