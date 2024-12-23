using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    [Table("Comments")] //this is the actual table names but u dont have to do this (many to many de table isimlerini karıştırıp sorun çıkarabiliyor bazen)
    public class Comment
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
       
        
        public int? StockId { get; set; }//this navigation property
        
        public Stock? Stock { get; set; } //we are trying together with stock with what is called CONVENTİON (entity framework is wiring for us)
        

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; } 
    }
}