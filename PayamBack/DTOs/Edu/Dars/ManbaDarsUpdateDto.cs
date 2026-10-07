using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Dars
{
    public class ManbaDarsUpdateDto : ManbaDarsCreateDto
    {
        /// <summary>شناسه منبع. 0 برای منبع جدید، > 0 برای ویرایش منبع موجود</summary>
        public int Id { get; set; }
    }
}