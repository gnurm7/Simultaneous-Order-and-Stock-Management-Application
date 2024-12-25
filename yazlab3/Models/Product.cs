namespace yazlab3.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }

        // Navigation property
        public ICollection<Order>? Orders { get; set; }  // Nullable yapıldı
       
    }
}