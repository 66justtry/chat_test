using System.ComponentModel.DataAnnotations;

namespace chat_test.Models
{
    public class Message
    {

        public Message(string sender, string receiver, string text)
        {
            this.sender = sender;
            this.receiver = receiver;
            this.text = text;
        }


        [Key]
        public int id { get; set; }

        [Required]
        public string sender { get; set; } //login

        [Required]
        public string receiver { get; set; } //login

        [Required]
        public string text { get; set; } //message text

        public int checked_message { get; set; } = 0; //for checked messages - to add then

        public DateTime time { get; set; } = DateTime.Now; //sending time

        public int deleted { get; set; } = 0; //if the message is deleted only for user - 1, else - 0

        public int answerto_id { get; set; } = 0; //id of answered message
    }
}
