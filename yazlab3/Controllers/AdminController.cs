using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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
    //public void bilmemne()
    //{
    //    //işlem x
    //}
    public string BuyProduct(int customerId, int productId, int quantity, int? orderId)
    {
        

        // Kullanıcıyı al
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        if (customer == null)
        {
            return "Kullanıcı bulunamadı.";
        }

        // Ürünü al
        var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
        if (product == null || product.Stock < quantity)
        {
            return "Ürün bulunamadı veya yetersiz stok.";
        }

        // Toplam fiyatı hesapla
        double totalPrice = (double)(product.Price * quantity);

        // Kullanıcı bütçesini kontrol et
        if (customer.Budget < totalPrice)
        {
            new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "Müşteri bütcesi yetersiz.");
            return "Bütçe yetersiz.";
        }

        // Stok düş, bütçeden düş ve toplam harcamayı güncelle
        product.Stock -= quantity;
        customer.Budget -= totalPrice;
        customer.TotalSpent += totalPrice;

        _context.SaveChanges();


        // bu gerçek bir log işlemi //log işlemi burasııııı
        new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId,Logger.UserType.Admin ,"Bilgilendirme", "Satın alma başarılı.approveorderdaki buy içinde");//aynı context mesela 500 insert falan yaparsak elbet syncstate yersin


        //new Thread(new ThreadStart(() => { ).Start();

        //  new Thread(new ThreadStart(() =>
        //  {//Şimdi o kadar 
        //      new Logger.Log(customerID.Value, "Sepete Ekeleme İşlemi Yapıldı! Yapılan Tarih:" + DateTime.Now, Logger.UserType.Musteri);//Gördün mü mesela burda fonksiyonu süsleyedebilirsin farklı parametrelerde gönderebilirsin sana kalmış ben mesela tarih'i stringe ekledim sen onu ayrı alana basmak istersen ayrı parametre olarak gönder keyfine göre 
        //  })
        //  ).Start();
        return "Ürün satın alındı.";
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
            double waitTimeInSeconds = existingOrder.WaitTime.TotalSeconds;//tüm zamanı saniiye olarak al demek TotalMinutes, TotalHours, TotalDays
            existingOrder.OrderPriority = (int)(basePriority + (waitTimeInSeconds * waitTimeWeight));

            // Müşterinin öncelik skorunu güncelle  
            customer.PriorityScore = existingOrder.OrderPriority;
        }

        _context.SaveChanges();

        // Müşteri ve sipariş nesnelerinin null olmadığından emin olun
        if (customer != null && existingOrder != null)
        {
            // Sipariş onaylandıktan sonra ürün satın alma işlemini çağır
            string purchaseResult = BuyProduct(customer.CustomerID, existingOrder.ProductID, existingOrder.Quantity,existingOrder.OrderID);
            ViewBag.PurchaseMessage = purchaseResult;  // Mesajı ViewBag'e ekle
        }
        else
        {
            ViewBag.PurchaseMessage = "Müşteri veya sipariş bulunamadı.";
        }
        new Logger.Log(HttpContext.Session.GetInt32("CustomerID"), orderId, Logger.UserType.Admin, "Bilgilendirme", "admin onayladı Satın alma başarılı.approveorderda");
        return RedirectToAction("OrderList"); // Sipariş listesi sayfasına yönlendir  
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

}