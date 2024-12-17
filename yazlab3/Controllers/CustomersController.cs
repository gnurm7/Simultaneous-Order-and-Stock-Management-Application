using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yazlab3.Models;

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

    return View(viewModel);
}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Buy(int ProductID, int Quantity)
        {
            // Giriş yapan kullanıcıyı session'dan al
            int? customerID = HttpContext.Session.GetInt32("CustomerID");

            if (customerID == null)
            {
                // Eğer kullanıcı giriş yapmamışsa Login sayfasına yönlendir
                return RedirectToAction("Login", "Customers");
            }

            // Kullanıcıyı al
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Ürünü al
            var product = _context.Products.FirstOrDefault(p => p.ProductID == ProductID);
            if (product == null || product.Stock < Quantity)
            {
                return BadRequest("Ürün bulunamadı veya yetersiz stok.");
            }

            // Ürün stoğunu azalt
            product.Stock -= Quantity;

            // Toplam fiyatı hesapla
            double totalPrice = (double)(product.Price * Quantity);

            // Kullanıcı bütçesini kontrol et
            if (customer.Budget < totalPrice)
            {
                return BadRequest("Bütçe yetersiz.");
            }

            // Kullanıcının bütçesinden düş ve toplam harcamasını güncelle
            customer.Budget -= totalPrice;
            customer.TotalSpent += totalPrice;

            // Değişiklikleri kaydet
            _context.SaveChanges();

            // Başarılı satın alma sonrası My sayfasına yönlendir
            return RedirectToAction("My");
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

            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction("OrderStatus");
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
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }
    }
}
