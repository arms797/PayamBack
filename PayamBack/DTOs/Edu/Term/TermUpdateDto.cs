using System.ComponentModel.DataAnnotations;

public class TermUpdateDto
{
    [MaxLength(100)]
    public string? OnvanTerm { get; set; }
    [MaxLength(50)]
    public string? Nimsal { get; set; }
    [MaxLength(50)]
    public string? SalTahsili { get; set; }

    public DateOnly? TermJariShoroo { get; set; }

    public DateOnly? TermJariPayan { get; set; }

    public DateOnly? TarikheDastrasi { get; set; }
    public DateOnly? TarikheEraeeDars { get; set; }
    public DateOnly? TarikhePayanDars { get; set; }
    public DateOnly? TarikheShorooClass { get; set; }
    public DateOnly? TarikhePayanClass { get; set; }
    public DateOnly? TarikheShorooMojavezMarakez { get; set; }
    public DateOnly? TarikhePayanMojavezMarakez { get; set; }

    public bool? Vazeeyat { get; set; }

    public bool? IsHaftegiRequired { get; set; }
}