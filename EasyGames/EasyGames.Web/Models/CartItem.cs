namespace EasyGames.Web.Models;

// Minimal cart line kept in session
public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Qty { get; set; }
}
