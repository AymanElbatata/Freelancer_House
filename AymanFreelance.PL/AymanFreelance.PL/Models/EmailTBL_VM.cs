using AymanFreelance.DAL.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace AymanFreelance.PL.Models
{
    public class EmailTBL_VM : BaseEntity<int>
    {
        public string? From { get; set; } = "aymanelbatata.net@gmail.com";
        public string? To { get; set; } = null!;
        public string? Subject { get; set; } = null!;
        public string? Body { get; set; } = null!;
    }
}
