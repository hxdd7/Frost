using System.Linq;

namespace Frost
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService()
        {
            _context = new AppDbContext();
        }

        // Register a new user
        public bool Register(string username, string email, string password, out string errorMessage)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                errorMessage = "Username already exists!";
                return false;
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                errorMessage = "Email already registered!";
                return false;
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            errorMessage = string.Empty;
            return true;
        }

        // Login a user
        public bool Login(string usernameOrEmail, string password, out string errorMessage)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null)
            {
                errorMessage = "User not found!";
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                errorMessage = "Invalid password!";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
