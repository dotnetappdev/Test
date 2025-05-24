using System.Text.Json.Serialization;

namespace ShoppingCart.Data.Entity
{
    public class CartItem
    {
        public int ID { get; set; }
        public int CartID { get; set; }
        [JsonIgnore]
        public Cart Cart { get; set; }
        public int ProductID { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
