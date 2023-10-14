namespace Xk7.Model
{
    public class SftpSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 22;
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "root";
    }
}