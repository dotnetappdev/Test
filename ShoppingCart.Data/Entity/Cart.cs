namespace ShoppingCart.Data.Entity
{
    public class Cart
    {
        public int ID { set; get; }
        public string CustomerName { set; get; } = String.Empty;
        public ICollection<CartItem> CartItems { get; set; }
        public decimal TotalAmount { set; get; }
    }
}
