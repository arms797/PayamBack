using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class ServiciDetailes
    {
        [Key]
        public int Id { get; set; }
        public int ServiciId { get; set; }
        public int DarsId { get; set; }

        [ForeignKey(nameof(ServiciId))]
        public Servici Servici { get; set; }
        [ForeignKey(nameof(DarsId))]
        public Dars Dars { get; set; }
    }
}
