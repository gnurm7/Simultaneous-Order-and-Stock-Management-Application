namespace yazlab3.Models
{
    public class CustomerProductViewModel
    {
        public Customer Customer { get; set; } // Kullanıcı bilgileri
        public IEnumerable<Product> Products { get; set; } // Ürünler
    }
}
