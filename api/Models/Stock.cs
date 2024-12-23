using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace api.Models
{
    [Table("Stocks")]
    public class Stock
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty; //to not get no refference errors
        public string CompanyName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")] //we need to make sure when we input a decimal that is only monetary amount 18 digit and 2 decimal places
        public decimal Purchase { get; set; }

         [Column(TypeName = "decimal(18,2)")]
        public decimal LastDiv { get; set; } //to correctly mathes the financial modelling prep Api (for react)
        
        public string Industry { get; set; } = string.Empty;
        public long MarketCap { get; set; } //entire value of the company

        public List<Comment> Comments { get; set; } = new List<Comment>();  //one to many 
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>();

    
    }
}