using Models.DBModel;
namespace Models.DTOModel
{
    public class UserViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public User User { get; set; }
    }
}
