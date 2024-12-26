using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using yazlab3.Models;
using Logger = yazlab3.Controllers.LogController;

public class AdminController : Controller
{
    private readonly Context _context;

    public AdminController(Context context)
    {
        _context = context;
    }

    public async Task<IActionResult> ViewCharts()
    {
        var stokVerileri = await _context.Products
            .Select(p => new Product
            {
                ProductName = p.ProductName,
                Stock = p.Stock,
                Price = p.Price
            })
            .ToListAsync();

        // Veriyi ViewBag ile JSON formatında gönderiyoruz
        ViewBag.StokVerileri = JsonConvert.SerializeObject(stokVerileri);  // JSON'a çeviriyoruz

        return View();
    }
    public IActionResult Index()
    {
        return View();
    }
    // Sipariş listesini gösteren metod  
    // Sipariş listesini gösteren metod  
    public IActionResult OrderList()
    {
        // Tüm siparişleri müşteri ve ürün bilgileriyle birlikte getir
        var orders = _context.Orders
            .Include(o => o.Customer) // Müşteri bilgilerini dahil et
            .Include(o => o.Product)  // Ürün bilgilerini dahil et
            .ToList();

        // Bekleme süresi ve öncelik değerlerini hesapla
        foreach (var order in orders)
        {
            if (order.OrderStatus == "Siparişiniz Alındı") // Sadece bekleyen siparişler için hesaplama yap
            {
                // Bekleme süresini güncelle
                order.WaitTime = DateTime.Now - order.OrderDate;

                // Öncelik puanını güncelle
                var customer = order.Customer;
                if (customer != null)
                {
                    int basePriority = customer.CustomerType == "Premium" ? 15 : 10;
                    double waitTimeWeight = 0.5; // Bekleme süresi ağırlığı
                    double waitTimeInSeconds = order.WaitTime.TotalSeconds;

                    order.OrderPriority = (int)(basePriority + (waitTimeInSeconds * waitTimeWeight));
                    customer.PriorityScore = order.OrderPriority; // Müşteri öncelik skorunu da güncelle
                }
            }
        }

        // Değişiklikleri veritabanına kaydet
        _context.SaveChanges();

        // Siparişleri öncelik değerine göre sırala ve View'a gönder
        var orderedOrders = orders.OrderByDescending(o => o.OrderPriority).ToList();
        return View(orderedOrders);
    }
    public IActionResult ApproveOrder(int orderId)
    {
        var existingOrder = _context.Orders
            .Include(o => o.Customer)  // Müşteri bilgisini dahil et
            .Include(o => o.Product)   // Ürün bilgisini dahil et
            .FirstOrDefault(o => o.OrderID == orderId);

        if (existingOrder == null)
        {
            return NotFound("Sipariş bulunamadı.");
        }

        // Siparişin durumunu ve onay tarihini güncelle
        existingOrder.OrderStatus = "Onaylandı";
        existingOrder.ApprovalDate = DateTime.Now; // Onay tarihi kaydedilir  
        existingOrder.WaitTime = existingOrder.ApprovalDate - existingOrder.OrderDate; // WaitTime'ı hesapla  

        // Müşteri bilgilerini al  
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == existingOrder.CustomerID);
        if (customer != null)
        {
            // Temel öncelik skorunu belirle
            int basePriority = customer.CustomerType == "Premium" ? 15 : 10;
            double waitTimeWeight = 0.5;
            double waitTimeInSeconds = existingOrder.WaitTime.TotalSeconds; // tüm zamanı saniye olarak al
            existingOrder.OrderPriority = (int)(basePriority + (waitTimeInSeconds * waitTimeWeight));

            // Müşterinin öncelik skorunu güncelle  
            customer.PriorityScore = existingOrder.OrderPriority; // her siparişte değişiyor
        }

        _context.SaveChanges();

        // Müşteri ve sipariş nesnelerinin null olmadığından emin olun
        if (customer != null && existingOrder != null)
        {
            // Sipariş onaylandıktan sonra "Siparişiniz hazırlanıyor." mesajını ekliyoruz
            ViewBag.OrderStatusMessage = "Siparişiniz hazırlanıyor.";
        }
        else
        {
            ViewBag.OrderStatusMessage = "Müşteri veya sipariş bulunamadı.";
        }

        new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "admin onayladı, satın alma başarılı.");

        return RedirectToAction("OrderList"); // Sipariş listesi sayfasına yönlendir  
    }

    // Dinamik olarak bekleme süresi ve öncelik hesaplama
    [HttpGet]
    public JsonResult GetUpdatedOrders()
    {
        var orders = _context.Orders
            .Where(o => o.OrderStatus != "Onaylandı") // Onaylanmayan siparişler
            .ToList();

        var updatedOrders = orders.Select(o => new
        {
            o.OrderID,
            WaitTime = (DateTime.Now - o.OrderDate).TotalMinutes, // Bekleme süresi
            o.OrderPriority
        }).ToList();

        return Json(updatedOrders);
    }

    // Siparişi reddetme metodunu tanımlama  
    public IActionResult RejectOrder(int orderId)
    {
        var existingOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
        if (existingOrder == null)
        {
            return NotFound("Sipariş bulunamadı.");
        }

        // Siparişin durumunu ve reddedilme tarihini güncelle  
        existingOrder.OrderStatus = "Reddedildi";//ay yukarıya yapmısım sadece reddette yok Şuan beynim offline benim algoritmik bişey sorma kodluk bişey varsa onu sor :( hee tamaam doğru diyelim bu işleme şimdi threadleri fln nasıl yapım devamı aklıma gelmio loglama fln
        existingOrder.ApprovalDate = DateTime.Now; // Reddetme tarihi kaydedilir  
       existingOrder.WaitTime = existingOrder.ApprovalDate - existingOrder.OrderDate; // WaitTime'ı hesapla
        // existingOrder.WaitTime = new DateTime((existingOrder.ApprovalDate - existingOrder.OrderDate).Ticks);
                                                                                       // Değişiklikleri kaydet  ABLA simdi bu hesaplama doğru mu date timelerı birbirinden çıkarıom veri tabanında söyle kaydedilio su virgülden sonrakiler hesaplamada sorun cıkarır mı yuvarlama yap tmam peki ben oncelik sıralamasını doğru mu anlamısım 
        _context.SaveChanges();
        new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "Sipariş reddedildi.");
        return RedirectToAction("OrderList"); // Sipariş listesi sayfasına yönlendir  
    }
    public async Task<IActionResult> ViewLogs()
    {
        var logs = await _context.Logs
              .OrderByDescending(l => l.LogDate)
              .Take(20)
             .ToListAsync();

        var logViewModels = new List<LogViewModel>();

        foreach (var log in logs)
        {
            var logViewModel = new LogViewModel
            {
                LogID = log.LogID,
                CustomerID = log.CustomerID,
                OrderID = log.OrderID,
                LogType = log.LogType,
                LogDetails = log.LogDetails,
                LogDate = log.LogDate,
            };
            if (log.CustomerID != null)
            {
                var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerID == log.CustomerID);

                if (customer != null)
                {
                    logViewModel.CustomerType = customer.CustomerType;
                    if (log.LogDetails == "Ürün sepete eklendi.")
                    {
                        var latestOrder = await _context.Orders
                                            .Include(o => o.Product)
                                             .Where(o => o.CustomerID == log.CustomerID && o.OrderStatus == "Siparişiniz Alındı")
                                          .OrderByDescending(o => o.AddedToCartTime)
                                        .FirstOrDefaultAsync();
                        if (latestOrder != null)
                        {
                            logViewModel.ProductName = latestOrder.Product.ProductName;
                            logViewModel.Quantity = latestOrder.Quantity;
                        }
                    }
                }

            }
            if (log.OrderID != null)
            {
                var order = await _context.Orders.Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.OrderID == log.OrderID);
                if (order != null)
                {
                    logViewModel.ProductName = order.Product.ProductName;
                    logViewModel.Quantity = order.Quantity;
                }
            }

            logViewModels.Add(logViewModel);
        }
        return View(logViewModels);
    }
    [HttpPost]
    public IActionResult ProcessAllOrders()
    {
        var pendingOrders = _context.Orders
            .Include(o => o.Customer)  // Müşteri bilgilerini dahil et
            .Include(o => o.Product)   // Ürün bilgilerini dahil et
            .Where(o => o.OrderStatus == "Siparişiniz Alındı")  // Sadece bekleyen siparişleri al
            .OrderByDescending(o => o.OrderPriority) // Önceliğe göre sırala
            .ToList();

        foreach (var order in pendingOrders)
        {
            var product = order.Product;
            var customer = order.Customer;

            if (product.Stock >= order.Quantity && (int)(customer.Budget) >= order.TotalPrice)
            {
                // Stok ve bütçe güncelleniyor
                product.Stock -= order.Quantity;
                customer.Budget -= (double)order.TotalPrice;
                customer.TotalSpent += (double)order.TotalPrice;

                order.OrderStatus = "Onaylandı";
                order.ApprovalDate = DateTime.Now;
                order.WaitTime = order.ApprovalDate - order.OrderDate;
                //log burda olduğu için onaylanınca ürrün ismi gözükmücek çünkü hepsiinni onalıyor
                new Logger.Log(HttpContext.Session.GetInt32("AdminID"), order.OrderID, Logger.UserType.Admin, "Bilgilendirme", "Sipariş onaylandı ve işleme alındı.");
            }
            else
            {
                order.OrderStatus = "Reddedildi";
                new Logger.Log(HttpContext.Session.GetInt32("AdminID"), order.OrderID, Logger.UserType.Admin, "Bilgilendirme", "Sipariş reddedildi. Yetersiz stok veya bütçe.");
            }
        }

        _context.SaveChanges(); // Değişiklikleri kaydet
        ViewBag.OrderStatusMessage = "Tüm işlemler sırayla tamamlandı.";

        return RedirectToAction("OrderList"); // Sipariş listesine geri dön
    }

}