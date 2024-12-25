﻿using Microsoft.AspNetCore.Mvc;
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
    public IActionResult OrderList()
    {//inculude ile her şeyi getirebilioz
        // Müşteri ve Ürün bilgileri ile birlikte siparişleri al  
        var orders = _context.Orders
            .Include(o => o.Customer) // Müşteri bilgilerini dahil et  
            .Include(o => o.Product)   // Ürün bilgilerini dahil et  
            .ToList(); // Tüm siparişleri al  

     //   new Thread(new ThreadStart(bilmemne)).Start();//Örnek veriyorum burda istediğin yerde çağırabilirsin keyfine göre mesela müsteri satın aldığı zaman çağırım b,herhangi contolreer önemli mi
        return View(orders);
    }


    //    // bu gerçek bir log işlemi //log işlemi burasııııı
    //    new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId,Logger.UserType.Admin ,"Bilgilendirme", "Satın alma başarılı.approveorderdaki buy içinde");//aynı context mesela 500 insert falan yaparsak elbet syncstate yersin


    //    //new Thread(new ThreadStart(() => { ).Start();

    //    //  new Thread(new ThreadStart(() =>
    //    //  {//Şimdi o kadar 
    //    //      new Logger.Log(customerID.Value, "Sepete Ekeleme İşlemi Yapıldı! Yapılan Tarih:" + DateTime.Now, Logger.UserType.Musteri);//Gördün mü mesela burda fonksiyonu süsleyedebilirsin farklı parametrelerde gönderebilirsin sana kalmış ben mesela tarih'i stringe ekledim sen onu ayrı alana basmak istersen ayrı parametre olarak gönder keyfine göre 
    //    //  })
    //    //  ).Start();
    //    return "Ürün satın alındı.";
    //}
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
    public IActionResult ViewLogs()
    {
        var logs = _context.Logs
                           .OrderByDescending(l => l.LogDate)  // Logları en yeni olanı en üstte olacak şekilde sırala
                           .Take(20)  // Son 20 logu al
                           .ToList();

        return View(logs);  // View'a logları gönder
    }
    [HttpPost]
    public IActionResult ProcessAllOrders()
    {
        var pendingOrders = _context.Orders
            .Include(o => o.Customer)  // Müşteri bilgilerini dahil et
            .Include(o => o.Product)   // Ürün bilgilerini dahil et
            .Where(o => o.OrderStatus == "Sepette")  // Sadece bekleyen siparişleri al
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