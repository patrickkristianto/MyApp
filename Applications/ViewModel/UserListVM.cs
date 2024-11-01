using Applications.Models;

namespace Applications.ViewModel
{
    public class UserListVM
    {
        public Users User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
