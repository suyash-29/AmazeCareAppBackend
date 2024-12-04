using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class Test
    {
        [Key]
        public int TestID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TestName { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TestPrice { get; set; }

        public ICollection<MedicalRecordTest> MedicalRecordTests { get; set; }
    }
}
