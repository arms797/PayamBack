using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermDeleteDto
    {
        [Required]
        public int Id { get; set; }
    }
}