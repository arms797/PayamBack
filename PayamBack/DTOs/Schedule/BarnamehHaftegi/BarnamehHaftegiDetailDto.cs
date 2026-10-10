using PayamBack.Controllers.Schedule;
using PayamBack.DTOs.Schedule.Hamjavar;
using PayamBack.Models.Core;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiDetailDto
    {
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadLastName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public Markaz OstadMarkaz { get; set; }
        public string CodeTerm { get; set; } = string.Empty;

        // ============================================================
        // 🔥 فیلدهای جدید برای کارت اطلاعات استاد
        // ============================================================
        public string? Reshteh { get; set; }           // از OstadMadrak (PishFarz == true)
        public string? Maghta { get; set; }            // از OstadMadrak (PishFarz == true)
        public string? MartabehElmi { get; set; }      // از Ostad
        public string? PostEjraei { get; set; }        // از ElmiTerm (فعال یا تأییدشده)
        public decimal? VahedMovazafi { get; set; }
        public string? Mobile { get; set; }            // از Ostad
        public int NoeHamkari { get; set; }            // از Ostad


        public int? NazarElmi { get; set; }
        public string NazarElmiDisplay { get; set; } = string.Empty;
        public int? NazarRaeisMarkaz { get; set; }
        public string NazarRaeisMarkazDisplay { get; set; } = string.Empty;
        public int? NazarModirGrooh { get; set; }
        public string NazarModirGroohDisplay { get; set; } = string.Empty;
        public int? NazarMoaven { get; set; }
        public string NazarMoavenDisplay { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string? ModirGroohNaam { get; set; } = string.Empty;
        public string? RaeisMarkazNaam { get; set; } = string.Empty;
        public string? MoavenNaam { get; set; } = string.Empty;


        public string ApproveStatus { get; set; } = string.Empty;
        public string ApproveStatusDisplay { get; set; } = string.Empty;

        public DateTime? TarikhElmi { get; set; }
        public DateTime? TarikhModirGrooh { get; set; }
        public DateTime? TarikhMoaven { get; set; }

        public int TotalSessions { get; set; }
        public int RequiredSessions { get; set; }
        public int RequiredHours { get; set; }
        public bool IsComplete { get; set; }

        public int? OstadUserId { get; set; }
        public int? UserIdModirGrooh { get; set; }
        public int? UserIdRaeisMarkaz { get; set; }
        public int? UserIdMoaven { get; set; }

        public SignatureDto? SignatureOstad { get; set; }
        public SignatureDto? SignatureModirGrooh { get; set; }
        public SignatureDto? SignatureRaeisMarkaz { get; set; }
        public SignatureDto? SignatureMoaven { get; set; }

        public List<BarnamehHaftegiDetailItemDto> Details { get; set; } = new();
    }
}
