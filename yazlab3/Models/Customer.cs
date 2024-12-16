using System.ComponentModel.DataAnnotations;

namespace yazlab3.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string KullaniciAdi { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Sifre { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Eposta { get; set; }

        public double Budget { get; set; }
        public string CustomerType { get; set; } // "Premium" veya "Standard"
        public double TotalSpent { get; set; }

        public ICollection<Order>? Orders { get; set; }//nullable olarak ayarla işilkiler olduğu için veritabannına kaydetmiyır
    }

}
