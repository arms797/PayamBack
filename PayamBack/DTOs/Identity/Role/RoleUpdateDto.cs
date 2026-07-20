namespace PayamBack.DTOs.Identity.Role
{
    public class RoleUpdateDto
    {
        public string? Name { get; set; }
        public int? CodeRole { get; set; }
        public bool? Vazeeyat { get; set; }
        public bool? Emza { get; set; }
        public bool? IsAdmin { get; set; }  // ← اضافه شد
    }
}