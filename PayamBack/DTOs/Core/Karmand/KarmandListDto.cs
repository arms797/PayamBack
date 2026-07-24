// در فایل DTOs/Core/Karmand/KarmandListDto.cs
namespace PayamBack.DTOs.Core.Karmand
{
    public class KarmandListDto
    {
        public int Id { get; set; }                // ← Id کارمند (Karmand.Id)
        public int? UserId { get; set; }            // ← 🔥 اضافه شد: Id کاربر (AppUser.Id)
        public string CodeMelli { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaameKhanevadeghi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }
        public bool? VazeeatMovaghat { get; set; }  // ← اضافه شد (برای تغییر وضعیت)
    }
}