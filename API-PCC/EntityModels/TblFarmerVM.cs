using System.ComponentModel.DataAnnotations.Schema;

namespace API_PCC.EntityModels
{
    public class TblFarmerVM
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public string TelephoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public int User_Id { get; set; }
        public int Group_Id { get; set; }
        public bool Is_Manager { get; set; }
        public int FarmerClassification_Id { get; set; }
        public int FarmerAffliation_Id { get; set; }
        public List<string> FarmerBreedTypes { get; set; }
        public List<string> FarmerFeedingSystems { get; set; }
        //public virtual ICollection<TblFarmerBreedType>? FarmerBreedTypes { get; set; } = new List<TblFarmerBreedType>();
        //public virtual ICollection<FarmerFeedingSystem>? FarmerFeedingSystems { get; set; } = new List<FarmerFeedingSystem>();
        public int Created_By { get; set; }
        public DateTime Created_At { get; set; }
        public int? Updated_By { get; set; }
        public DateTime Updated_At { get; set; }
        public int? Deleted_By { get; set; }
        public string Email { get; set; }
        public DateTime? Deleted_At { get; set; }
        public bool Is_Deleted { get; set; }

    }
}
