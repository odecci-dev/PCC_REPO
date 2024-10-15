namespace API_PCC.ApplicationModels
{
    public class FarmerSearchFilterModel
    {
        public string? searchValue { get; set; }
        public List<string>? breedType { get; set; }
        public List<string>? feedingSystem { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
}

