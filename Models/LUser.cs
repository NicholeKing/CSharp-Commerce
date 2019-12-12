using System;
using System.ComponentModel.DataAnnotations;

namespace Commerce.Models
{
    public class LUser
    {
        [Required(ErrorMessage="Email is required")]
        public string LEmail {get;set;}

        [Required(ErrorMessage="Password is required")]
        [DataType(DataType.Password)]
        public string LPassword {get;set;}
    } 
}