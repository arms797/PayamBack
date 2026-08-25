public class TermDetailDto
{
    public string CodeTerm { get; set; } = string.Empty;
    public string OnvanTerm { get; set; } = string.Empty;
    public DateOnly? TermJariShoroo { get; set; }
    public DateOnly? TermJariPayan { get; set; }
    public DateOnly? TarikheDastrasi { get; set; }
    public DateOnly? TarikheEraeeDars { get; set; }
    public DateOnly? TarikhePayanDars { get; set; }
    public DateOnly? TarikheShorooClass { get; set; }
    public DateOnly? TarikhePayanClass { get; set; }
    public DateOnly? TarikheShorooMojavezMarakez { get; set; }
    public DateOnly? TarikhePayanMojavezMarakez { get; set; }
    public bool Vazeeyat { get; set; }
    public bool IsHaftegiRequired { get; set; }
}