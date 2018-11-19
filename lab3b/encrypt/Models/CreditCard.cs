using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace encrypt.Models
{
    public class CreditCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { set; get; }

        [NotMapped]
        [Required]
        public string PTCC { set; get; }
        //[Required]
        public string ECC { set; get; }
        //[Required]
        public string SECC { set; get; }

        public CreditCard()
        {

        }
    }
}