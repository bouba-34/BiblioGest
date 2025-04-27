using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioGest.Models
{
    public class Loan
    {
        [Key] public int Id { get; set; }

        public int BookId { get; set; }

        [ForeignKey("BookId")] public virtual Book Book { get; set; }

        public int MemberId { get; set; }

        [ForeignKey("MemberId")] public virtual Member Member { get; set; }

        public DateTime LoanDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public bool IsReturned
        {
            get { return ReturnDate.HasValue; }
        }

        public bool IsOverdue
        {
            get { return !IsReturned && DateTime.Now > DueDate; }
        }

        public Loan()
        {
            LoanDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(14); // Default loan period: 14 days
        }
    }
}