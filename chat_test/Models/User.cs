using System.ComponentModel.DataAnnotations;

namespace chat_test.Models
{
    public class User
    {
        public User(string login, string username)
        {
            this.login = login;
            this.username = username;
        }
        //public User(string login, string username, string chats)
        //{
        //    this.login = login;
        //    this.username = username;
        //    this.chats = chats;
        //}

        [Key]
        public string login { get; set; } //table key - login

        [Required]
        public string username { get; set; } = "New user"; //custom username in chats

        public string? password { get; set; } //for using in the future

        public string? chats { get; set; } //array of logins
    }
}
