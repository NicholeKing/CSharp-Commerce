using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Commerce.Models
{
    public class Transaction
    {
        [Key]
        public int TranId {get;set;}
        public int UserId {get;set;}
        public User User {get;set;}
        public int PID {get;set;}
        public Product Product {get;set;}
    }
}