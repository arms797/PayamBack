using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class Servici
    {
        [Key]
        public int Id { get; set; }   
        public string Onvan { get; set; }
        public int MarkazId { get; set; }
        public int Level { get; set; } = 4;
        [ForeignKey(nameof(MarkazId))]
        public Markaz Markaz { get; set; }
    }
}
