using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TaghvimTermi
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CodeTerm { get; set; }

    [Required]
    [Column(TypeName = "date")]  // فقط تاریخ (بدون ساعت)
    public DateTime Tarikh { get; set; }

    [Required]
    public int CodeRooz { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoozHafteh { get; set; }

    [Required]
    public int CodeHafteh { get; set; }

    [Required]
    [MaxLength(50)]
    public string Hafteh { get; set; }

    [MaxLength(10)]
    public string? CodeSaateTatili { get; set; }

    [MaxLength(200)]
    public string? OnvanMonasebat { get; set; }

    [MaxLength(200)]
    public string? Tozihat { get; set; }

    [Required]
    public bool VazeeyatRoozha { get; set; } = true;
}
