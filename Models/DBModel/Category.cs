namespace Models.DBModel
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<BookCategories> BookCategories { get; set; } = new List<BookCategories>();
    }
}
