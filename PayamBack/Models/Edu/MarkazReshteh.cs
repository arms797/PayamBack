using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class MarkazReshteh
    {
        [Key]
        public int Id { get; set; }
        public int MarkazId { get; set; }
        public int ReshtehId { get; set; }
        public int? TedadDaneshjoo { get; set; }
        public bool Vazeeat { get; set; } = true;

        [ForeignKey(nameof(MarkazId))]
        public Markaz Markaz { get; set; }
        [ForeignKey(nameof(ReshtehId))]
        public Reshteh Reshteh { get; set; }
    }
}
