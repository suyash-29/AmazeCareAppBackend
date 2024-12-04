using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models
{
    public class MedicalRecordTest
    {
        [Key]
        public int RecordTestID { get; set; }

        [ForeignKey("MedicalRecord")]
        public int RecordID { get; set; }

        [ForeignKey("Test")]
        public int TestID { get; set; }

        public Test Test { get; set; }

        public MedicalRecord MedicalRecord { get; set; }
    }
}
