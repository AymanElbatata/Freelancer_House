using AymanFreelance.DAL.BaseEntity;

namespace AymanFreelance.PL.Models
{
    public class ProjectTBL_VM : BaseEntity<int>
    {
        public string? ProjectOwnerTBLId { get; set; }
        public virtual AppUser? ProjectOwnerTBL { get; set; }

        public string? ProjectFreelancerTBLId { get; set; }
        public virtual AppUser? ProjectFreelancerTBL { get; set; }

        public string? HashCode { get; set; } = null!;
        public string? Name { get; set; } = null!;
        public string? Image { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public int? RequiredDaysOfDelivery { get; set; } = 0;
        public int? TotalCost { get; set; } = 0;
        public int? PaymentInAdvance { get; set; } = 0;
        public bool IsPaymentInAdvancePaid { get; set; } = false;

        public DateTime? DateOfStartWork { get; set; } = null!;
        public bool IsDelivered { get; set; } = false;
        public DateTime? DateOfDelivery { get; set; } = null!;

        public int? IncomeProfit { get; set; } = 0;
    }
}
