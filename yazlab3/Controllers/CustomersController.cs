using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using yazlab3.Controllers.LogController;
using yazlab3.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Logger = yazlab3.Controllers.LogController;

namespace yazlab3.Controllers
{
    public class CustomersController : Controller
    {
        private readonly Context _context;

        public CustomersController(Context context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerID == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerID,CustomerName,Budget,CustomerType,TotalSpent,Eposta,KullaniciAdi,Sifre")] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                // Eğer ModelState geçerli değilse, hataları loglayın
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Hata mesajlarını loglayın
                }
                return View(customer);  // Hatalı formu tekrar göster
            }

            try
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login", "Customers");
            }
            catch (Exception ex)
            {
                // Hata mesajını yazdırarak sorunun ne olduğunu anlamaya çalışın
                Console.WriteLine($"Error occurred: {ex.Message}");
                // Geriye dön ve hata mesajı göster
                new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", " Yeni bir kullanıcı kayıt oldu.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Customers/Login
        public IActionResult Login()
        {
            return View(); // Login.cshtml sayfasını döndürür
        }

        // POST: Customers/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string KullaniciAdi, string Sifre)
        {
            // Kullanıcıyı veritabanında ara
            var customer = _context.Customers
                .FirstOrDefault(c => c.KullaniciAdi == KullaniciAdi && c.Sifre == Sifre);

            if (customer != null)
            {
                // Giriş başarılı: Kullanıcı ID'sini Session'a kaydet
                HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);
                 new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Kullanıcı Giriş yaptı.");
                // Kullanıcıyı My sayfasına yönlendir
                return RedirectToAction("MY", "Customers");
            }

            // Giriş başarısız: Hata mesajı göster
            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
           
            return View();
        }

        // GET: MY
        public IActionResult MY()
        {
            // Session'dan giriş yapan kullanıcının ID'sini al
            int? customerID = HttpContext.Session.GetInt32("CustomerID");

            if (customerID == null)
            {
                // Eğer kullanıcı giriş yapmamışsa Login sayfasına yönlendir
                return RedirectToAction("Login", "Customers");
            }

            // Kullanıcı bilgilerini al
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Ürünleri al
            var products = _context.Products.ToList();

            // Kullanıcı ve ürün bilgilerini View'e gönder
            var viewModel = new CustomerProductViewModel
            {
                Customer = customer,
                Products = products
            };
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Satın alma Sayfasına giriş başarılı.");
            return View(viewModel);
        }


        // POST: Customers/AddToCart
   [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddToCart(int ProductID, int Quantity)
{
    int? customerID = HttpContext.Session.GetInt32("CustomerID");
    if (customerID == null)
    {
        return RedirectToAction("Login", "Customers");
    }

    var product = _context.Products.FirstOrDefault(p => p.ProductID == ProductID);
    if (product == null || product.Stock < Quantity)
    {
        return BadRequest("Ürün bulunamadı veya stok yetersiz.");
    }

    var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerID);
            // Toplam fiyatı hesapla
            double totalPrice = (double)(product.Price * Quantity);

            if (customer.Budget < totalPrice)
    {
        return BadRequest("Bütçe yetersiz.");
    }

    //// Stok ve bütçe azalt
    //product.Stock -= Quantity;
    //customer.Budget -= totalPrice;

    // Sipariş oluştur
    var order = new Order
    {
        CustomerID = customerID.Value,
        ProductID = ProductID,
        Quantity = Quantity,
        TotalPrice = (decimal)totalPrice,
        OrderDate = DateTime.Now,
        AddedToCartTime = DateTime.Now,
        OrderStatus = "Sepette"
    };

    _context.Orders.Add(order);
    _context.SaveChanges();

    new Logger.Log(customerID, null, Logger.UserType.Customer, "Bilgilendirme", "Ürün sepete eklendi.");
    return RedirectToAction("Card");
}
        public IActionResult Card()
        {
            int? customerID = HttpContext.Session.GetInt32("CustomerID");
            if (customerID == null)
            {
                return RedirectToAction("Login", "Customers");
            }
            // Zaman aşımı kontrolü
            var now = DateTime.Now;
            var expiredOrders = _context.Orders
                .Where(o => o.CustomerID == customerID && o.OrderStatus == "Sepette")
                .ToList() // Verileri önce client'a al
                .Where(o => (now - o.AddedToCartTime).TotalMinutes > 5) // Sonrasında client tarafında hesapla
                .ToList();

            foreach (var order in expiredOrders)
            {
                // Stok iadesi
                var product = _context.Products.FirstOrDefault(p => p.ProductID == order.ProductID);
                if (product != null)
                {
                    product.Stock += order.Quantity;
                }

                // Siparişi kaldır
                _context.Orders.Remove(order);
                new Logger.Log(customerID, order.OrderID, Logger.UserType.Customer, "Bilgilendirme", "Sepetteki ürünün süresi dolduğu için sepetten çıkarıldı.");
            }

            _context.SaveChanges();

            // Kullanıcının siparişlerini getir
            var orders = _context.Orders
                .Where(o => o.CustomerID == customerID && o.OrderStatus == "Sepette")
                .Include(o => o.Product)
                .ToList();

            new Logger.Log(customerID, null, Logger.UserType.Customer, "Bilgilendirme", "Sepet görüntülendi.");

            return View(orders);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(List<int> productIds, List<int> quantities)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Customers");
            }

            // Kullanıcının sepetindeki siparişleri al
            var cartOrders = _context.Orders
                .Where(o => o.CustomerID == customerId && o.OrderStatus == "Sepette")
                .ToList();

            // Her bir siparişin durumunu "Siparişiniz Alındı" olarak güncelle
            foreach (var order in cartOrders)
            {
                order.OrderStatus = "Siparişiniz Alındı";
            }

            _context.SaveChanges();


            // Siparişleri işleme
            for (int i = 0; i < productIds.Count; i++)
            {
                int productId = productIds[i];
                int quantity = quantities[i];

                // Sipariş alma işlemi
                string result = BuyProduct(customerId.Value, productId, quantity);
                if (result != "Ürün satın alındı.")
                {
                    TempData["ErrorMessage"] = result;
                    return RedirectToAction("Card");
                }
            }

            // TempData mesajını set etme
            TempData["SuccessMessage"] = "Siparişiniz alındı. Admin onayını bekliyorsunuz.";

            return RedirectToAction("OrderStatus");
        }
        public string BuyProduct(int customerId, int productId, int quantity)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return "Kullanıcı bulunamadı.";
            }
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null || product.Stock < quantity)
            {
                return "Ürün bulunamadı veya yetersiz stok.";
            }
            double totalPrice = (double)product.Price * quantity;
            if (customer.Budget < totalPrice)
            {
                return "Bütçe yetersiz.";
            }
            product.Stock -= quantity;
            customer.Budget -= totalPrice;
            customer.TotalSpent += totalPrice;

            //Sipariş Oluşturma
            Order order = new Order()
            {
                CustomerID = customerId,
                ProductID = productId,
                Quantity = quantity,
                TotalPrice = product.Price * quantity,
                OrderDate = DateTime.Now,
                OrderStatus = "Siparişiniz Alındı",
                AddedToCartTime = DateTime.Now // Sepete eklenme zamanını ayarla
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            new Logger.Log(customerId, order.OrderID, Logger.UserType.Customer, "Bilgilendirme", "Sepete ürün eklendi.");
            return "Ürün satın alındı.";
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int OrderID)
        {
            int? customerID = HttpContext.Session.GetInt32("CustomerID");
            if (customerID == null)
            {
                return RedirectToAction("Login", "Customers");
            }

            // OrderID'nin geçerli olup olmadığını kontrol edin
            if (OrderID == 0)
            {
                return BadRequest("Geçersiz sipariş ID.");
            }

            // Sepetindeki ürünün bilgilerini al
            var order = await _context.Orders.Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.OrderID == OrderID && o.CustomerID == customerID);

            if (order == null)
            {
                return NotFound("Sepetinizde böyle bir ürün yok.");
            }

            // Ürünü ve siparişi bulduktan sonra, stok miktarını arttır
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == order.ProductID);

            if (product != null)
            {
                product.Stock += order.Quantity; // Sepetten çıkarılacak miktarı stoktan geri ekle
            }
            // Bütçeyi geri ekle
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerID);
            if (customer != null)
            {
                double totalPrice = (double)(order.TotalPrice); // Siparişin toplam fiyatı
                customer.Budget += totalPrice; // Bütçeye harcanan parayı geri ekle
            }
            // Siparişi sepetten kaldır
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            // Loglama işlemi
            new Logger.Log(customerID, order.OrderID, Logger.UserType.Customer, "Bilgilendirme", $"Sepetten {order.Product.ProductName} ürünü çıkarıldı.");


            return RedirectToAction("Card"); // Sepet sayfasına geri yönlendir
        }

        // GET: Customers/OrderStatus
        public IActionResult OrderStatus()
        {
            // Giriş yapan kullanıcının siparişlerini getir
            int? customerID = HttpContext.Session.GetInt32("CustomerID");
            if (customerID == null)
            {
                return RedirectToAction("Login", "Customers"); // Giriş yapılmadıysa Login'e yönlendir
            }

            var orders = _context.Orders
                .Where(o => o.CustomerID == customerID.Value)
                .Include(o => o.Product) // Ürün bilgilerini de çek
                .ToList();
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Müşteri Satın aldığı ürünler sayfasında.");
            return View(orders);
        }
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerID,CustomerName,Budget,CustomerType,TotalSpent,Eposta,KullaniciAdi,Sifre")] Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Kullanıcı bilgilerini güncelledi.");
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerID == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Kullanıcı Silindi.");
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }
    }
}
