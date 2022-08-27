﻿using System.ComponentModel.DataAnnotations;

namespace chat_test.Models
{
    public class Message
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string from { get; set; } //login

        [Required]
        public string to { get; set; } //login

        [Required]
        public string text { get; set; } //message text

        public int check { get; set; } //for checked messages - to add then

        public DateTime time { get; set; } = DateTime.Now; //sending time
    }
}