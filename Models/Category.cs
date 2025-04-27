using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioGest.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }

        [Required] [StringLength(50)] public string Name { get; set; }

        [StringLength(200)] public string Description { get; set; }

        public virtual ICollection<Book> Books { get; set; }

        public Category()
        {
            Books = new HashSet<Book>();
        }
    }
}