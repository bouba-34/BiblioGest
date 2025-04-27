using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioGest.Models
{
    public class Member
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(50)] public string FirstName { get; set; }

        [Required] [StringLength(50)] public string LastName { get; set; }

        [StringLength(200)] public string Address { get; set; }

        [EmailAddress] [StringLength(100)] public string Email { get; set; }

        [Phone] [StringLength(20)] public string Phone { get; set; }

        public DateTime RegistrationDate { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Loan> Loans { get; set; }

        public Member()
        {
            RegistrationDate = DateTime.Now;
            IsActive = true;
            Loans = new HashSet<Loan>();
        }

        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}