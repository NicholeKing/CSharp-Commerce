using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Commerce.Models
{
    public class Product
    {
        [Key]
        public int PID {get;set;}

        [Required]
        [MinLength(3, ErrorMessage="Your product name must be at least 3 characters")]
        public string Name {get;set;}

        [Required]
        [MaxLength(240, ErrorMessage="No one wants to read a novel")]
        public string Description {get;set;}

        [Required]
        [Range(0.00, 1000000000.00)]
        public double Price {get;set;}

        [Required]
        [Range(1,1000)]
        public int Quantity {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        public int UserId {get;set;}
        public User Seller {get;set;}
        public List<Transaction> Buyers {get;set;}
    }
}