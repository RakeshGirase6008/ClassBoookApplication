namespace ClassBookApplication.Models.PublicModel
{
    public class ClassListingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Rating { get; set; }
        public string Description { get; set; }
        public string EstablishmentDate { get; set; }
        public bool Favourite { get; set; }
    }
}
