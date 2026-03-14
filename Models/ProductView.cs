using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebFM_Style.Models;

public partial class ProductView
{
    public int Id { get; set; } 

    public int AccountId { get; set; }
    
    public int ProductId { get; set; } 

    public DateTime? ViewTime { get; set; } 
    
    public virtual Account? Account { get; set; }
    
    public virtual Product? Product { get; set; }
}