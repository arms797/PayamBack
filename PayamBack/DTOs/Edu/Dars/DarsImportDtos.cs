namespace PayamBack.DTOs.Edu.Dars
{
    // ============================================================
    // رکورد خام خوانده‌شده از یک ردیف اکسل
    // ============================================================
    public class DarsImportRow
    {
        public int RowNumber { get; set; }

        // ---- رشته ----
        public string? CodeReshte { get; set; }      // ستون 26
        public string? CodeMaghta { get; set; }      // ستون 27
        public string? TermVorood { get; set; }      // ستون 37
        public string? TermEamal { get; set; }       // ستون 38
        public string? ReshtehName { get; set; }     // ستون 34

        // ---- درس ----
        public string? CodeDars { get; set; }        // ستون 4
        public string? NaamDars { get; set; }        // ستون 5
        public decimal? VahedTeori { get; set; }     // ستون 6
        public decimal? VahedAmali { get; set; }     // ستون 7

        /// <summary>
        /// ستون 8 — هم SaatTeori و هم SaatTeoriOrginal از این خونده می‌شن.
        /// طبق توافق: در اکسل یک سلول مشترک داریم.
        /// </summary>
        public int? SaatTeori_File { get; set; }

        /// <summary>
        /// ستون 9 — هم SaatAmali و هم SaatAmaliOrginal از این خونده می‌شن.
        /// </summary>
        public int? SaatAmali_File { get; set; }

        public int? TermAkhz { get; set; }           // ستون 10
        public string? NoeDars { get; set; }         // ستون 11
        public string? NoeAzmoon { get; set; }       // ستون 12
        public int? Zarfiat { get; set; }            // ستون 28
        public string? TermUpdate { get; set; }      // ستون 3

        // ---- منبع ----
        public string? ShomareManba { get; set; }    // ستون 13
        public string? NoeManba { get; set; }        // ستون 14
        public string? OnvanManba { get; set; }      // ستون 15
        public string? Nevisandeh { get; set; }      // ستون 16
        public string? Motarjem { get; set; }        // ستون 17
        public string? SalEnteshar { get; set; }     // ستون 18
        public string? SalEntesharMiladi { get; set; } // ستون 19
        public string? Shabak { get; set; }          // ستون 20
        public string? Nasher { get; set; }          // ستون 21
        public string? NobateChap { get; set; }      // ستون 29
        public string? Vazeeyat { get; set; }        // ستون 22
        public string? CodePeyvast { get; set; }     // ستون 23
        public string? SharhPeyvast { get; set; }    // ستون 24
    }

    // ============================================================
    // ساختار درختی: رشته → درس → منبع
    // ============================================================
    public class ReshtehGroup
    {
        public string? CodeReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? TermVorood { get; set; }
        public string? TermEamal { get; set; }
        public string? ReshtehName { get; set; }
        public List<DarsWithManba> DarsList { get; set; } = new();
    }

    public class DarsWithManba
    {
        public string CodeDars { get; set; } = string.Empty;
        public string? NaamDars { get; set; }
        public decimal? VahedTeori { get; set; }
        public decimal? VahedAmali { get; set; }
        public int? SaatTeori_File { get; set; }
        public int? SaatAmali_File { get; set; }
        public int? TermAkhz { get; set; }
        public string? NoeDars { get; set; }
        public string? NoeAzmoon { get; set; }
        public int? Zarfiat { get; set; }
        public List<ManbaImportRow> ManbaList { get; set; } = new();
    }

    public class ManbaImportRow
    {
        public string? ShomareManba { get; set; }
        public string? NoeManba { get; set; }
        public string? Onvan { get; set; }
        public string? Nevisandeh { get; set; }
        public string? Motarjem { get; set; }
        public string? SalEnteshar { get; set; }
        public string? SalEntesharMiladi { get; set; }
        public string? Shabak { get; set; }
        public string? Nasher { get; set; }
        public string? NobateChap { get; set; }
        public string? Vazeeyat { get; set; }
        public string? CodePeyvast { get; set; }
        public string? SharhPeyvast { get; set; }
        public string? TermUpdate { get; set; }
    }
}
