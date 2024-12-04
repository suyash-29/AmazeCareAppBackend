using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazeCareAPI.Models
{

    public class DoctorSpecialization
    {
        public int DoctorID { get; set; }

        public int SpecializationID { get; set; }

        public Specialization Specialization { get; set; }
    }
}
