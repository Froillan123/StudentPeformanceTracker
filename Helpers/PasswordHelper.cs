namespace StudentPeformanceTracker.Helpers
{
    public static class PasswordHelper
    {
        // Hash a password using BCrypt
        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        // Verify a password against a hash
        public static bool VerifyPassword(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}