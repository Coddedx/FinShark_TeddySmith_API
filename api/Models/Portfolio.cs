using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    [Table("Portfolios")]
    public class Portfolio //many to many tablo but with the join table is more clean and more efficient so to it this way
    {
        // İNT THE DB CONTEXT WE ARE GONNA SET THE FOREİGN KEYS

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int StockId { get; set; }
        public Stock Stock { get; set; } 

        //THE GO TO THE APPUSER AND STOCK TABLE AND ADD PORTFOLİO LİST
    }
}