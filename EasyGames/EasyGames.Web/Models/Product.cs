using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding; // for BindNever

namespace EasyGames.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, StringLength(20)]
    public string Category { get; set; } = ""; // Book | Game | Toy

    [Range(0.01, 100000)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    // Read-only metadata: set by server, never bound from request
    [BindNever]
    [Display(Name = "Created")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
