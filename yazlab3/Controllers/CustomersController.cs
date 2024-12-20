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

                // Kullanıcıyı My sayfasına yönlendir
                return RedirectToAction("MY", "Customers");
            }

            // Giriş başarısız: Hata mesajı göster
            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Kullanıcı Giriş yaptı.");
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
            // Kullanıcı bilgilerini session'dan al
            int? customerID = HttpContext.Session.GetInt32("CustomerID");
            if (customerID == null)
            {
                return RedirectToAction("Login", "Customers"); // Giriş yapılmadıysa Login'e yönlendir
            }

            // Ürünü al
            var product = _context.Products.FirstOrDefault(p => p.ProductID == ProductID);
            if (product == null || product.Stock < Quantity)
            {
                return BadRequest("Ürün bulunamadı veya stok yetersiz.");
            }

            // Sipariş oluştur ve veritabanına kaydet
            var order = new Order
            {
                CustomerID = customerID.Value,
                ProductID = ProductID,
                Quantity = Quantity,
                TotalPrice = product.Price * Quantity,//TotalPrice = (decimal)(product.Price * Quantity),
                OrderDate = DateTime.Now,
                OrderStatus = "Onay Bekliyor"
            };
//product.Stock -= quantity;
        //    customer.Budget -= totalPrice;
        //    customer.TotalSpent += totalPrice;


            _context.Orders.Add(order);
            _context.SaveChanges();
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), null, Logger.UserType.Customer, "Bilgilendirme", "Ürün sepete eklendi.Müşterinin siparişi işleme alındı.");
            ////  Mutex
            //  new Thread(new ThreadStart(() =>
            //  {//Şimdi o kadar 
            //      new Logger.Log(customerID.Value, "Sepete Ekeleme İşlemi Yapıldı! Yapılan Tarih:" + DateTime.Now, Logger.UserType.Musteri);//Gördün mü mesela burda fonksiyonu süsleyedebilirsin farklı parametrelerde gönderebilirsin sana kalmış ben mesela tarih'i stringe ekledim sen onu ayrı alana basmak istersen ayrı parametre olarak gönder keyfine göre 
            //  })
            //  ).Start();
            return RedirectToAction("Card");
            //aynı kulllanıcın sipariş bilgisinide al sessionda tut 
        }
        public IActionResult Card(int ProductID,int Quantity)
        {//sessionlaarı unutma
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
            int? productID = HttpContext.Session.GetInt32("ProductID");
            //  ürünü tutalım
            var product = _context.Products.FirstOrDefault(p => p.ProductID == ProductID);
            if (product == null || product.Stock < Quantity)
            {
                return BadRequest("Ürün bulunamadı veya stok yetersiz.");
            }
            return View();
        }
        //public string BuyProduct(int customerId, int productId, int quantity, int? orderId)
        //{


        //    // Kullanıcıyı al
        //    var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        //    if (customer == null)
        //    {
        //        return "Kullanıcı bulunamadı.";
        //    }

        //    // Ürünü al
        //    var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
        //    if (product == null || product.Stock < quantity)
        //    {
        //        return "Ürün bulunamadı veya yetersiz stok.";
        //    }

        //    // Toplam fiyatı hesapla
        //    double totalPrice = (double)(product.Price * quantity);

        //    // Kullanıcı bütçesini kontrol et
        //    if (customer.Budget < totalPrice)
        //    {
        //        new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "Müşteri bütcesi yetersiz.");
        //        return "Bütçe yetersiz.";
        //    }

        //    // Stok düş, bütçeden düş ve toplam harcamayı güncelle
        //    product.Stock -= quantity;
        //    customer.Budget -= totalPrice;
        //    customer.TotalSpent += totalPrice;

        //    _context.SaveChanges();


        //    // bu gerçek bir log işlemi //log işlemi burasııııı
        //    new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "Satın alma başarılı.");//aynı context mesela 500 insert falan yaparsak elbet syncstate yersin


        //    //new Thread(new ThreadStart(() => { ).Start();

        //    //  new Thread(new ThreadStart(() =>
        //    //  {//Şimdi o kadar 
        //    //      new Logger.Log(customerID.Value, "Sepete Ekeleme İşlemi Yapıldı! Yapılan Tarih:" + DateTime.Now, Logger.UserType.Musteri);//Gördün mü mesela burda fonksiyonu süsleyedebilirsin farklı parametrelerde gönderebilirsin sana kalmış ben mesela tarih'i stringe ekledim sen onu ayrı alana basmak istersen ayrı parametre olarak gönder keyfine göre 
        //    //  })
        //    //  ).Start();
        //    return "Ürün satın alındı.";
        //}


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
