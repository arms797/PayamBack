namespace PayamBack.DTOs.Identity.Role
{
    public class RoleDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CodeRole { get; set; }
        public bool Vazeeyat { get; set; }
        public bool Emza { get; set; }
        public bool IsAdmin {  get; set; }
        public bool IsUniquePerMarkaz { get; set; } // ← اضافه شد

    }
}