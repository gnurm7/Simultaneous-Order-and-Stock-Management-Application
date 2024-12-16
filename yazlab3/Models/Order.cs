namespace yazlab3.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; } // Foreign Key
        public int ProductID { get; set; } // Foreign Key
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } // e.g., "Pending", "Completed"

        // Navigation properties
        public Customer Customer { get; set; }
        public Product Product { get; set; }
    }
}