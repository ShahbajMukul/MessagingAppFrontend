﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Payloads
{
    public class LoginPayload
    {
        [Required]
        public  string Username { get; set; }
        [Required]
        public  string Password { get; set; }
        public  string PublicKey { get; set; }
    }
}
