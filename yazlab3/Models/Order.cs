namespace yazlab3.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; } // Foreign Key
        public int ProductID { get; set; } // Foreign Key
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }//tarih ve saat sipariş oluşturulma tarihi

        public DateTime  ApprovalDate { get; set; } //siparişin onaylanma tarihi
        public string OrderStatus { get; set; } // e.g., "Pending", "Completed" //onay bekliyor,onaylandı reddedildi

        // Siparişin bekleme süresi=müşterinin bekleme süresi
        public TimeSpan WaitTime { get; set; }   //approvaldate-order date

        public int OrderPriority { get; set; }  // Siparişin önceliği customerdaki priority scorea göre belirlencek gereksizglb

          public DateTime AddedToCartTime { get; set; } // Sepete eklenme zamanı
        // Navigation properties
        public Customer Customer { get; set; }
        public Product Product { get; set; }
    }
}