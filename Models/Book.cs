using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioGest.Models
{
    public class Book
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(20)] public string ISBN { get; set; }

        [Required] [StringLength(200)] public string Title { get; set; }

        [Required] [StringLength(100)] public string Author { get; set; }

        [StringLength(100)] public string Publisher { get; set; }

        public int PublicationYear { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")] public virtual Category Category { get; set; }

        public int CopiesAvailable { get; set; }

        public virtual ICollection<Loan> Loans { get; set; }

        public Book()
        {
            Loans = new HashSet<Loan>();
        }
    }
}