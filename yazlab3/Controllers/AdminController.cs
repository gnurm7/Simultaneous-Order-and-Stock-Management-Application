using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using yazlab3.Models;

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
        return View(orders);
    }

    // Siparişi onaylama metodunu tanımlama  
    public IActionResult ApproveOrder(int orderId)
    {
        var existingOrder = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
        if (existingOrder == null)
        {
            return NotFound("Sipariş bulunamadı.");
        }

        // Siparişin durumunu ve onay tarihini güncelle  
        existingOrder.OrderStatus = "Onaylandı";
        existingOrder.ApprovalDate = DateTime.Now; // Onay tarihi kaydedilir  
        existingOrder.WaitTime = existingOrder.ApprovalDate - existingOrder.OrderDate; // WaitTime'ı hesapla  //yanlıs hesaplio glb 

        // Müşteri bilgilerini al  
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == existingOrder.CustomerID);
        if (customer != null)
        {
            // Temel öncelik skorunu belirle  
            int basePriority = customer.CustomerType == "Premium" ? 15 : 10;
            double waitTimeWeight = 0.5;
            double waitTimeInSeconds = existingOrder.WaitTime.TotalSeconds;
            existingOrder.OrderPriority = (int)(basePriority + (waitTimeInSeconds * waitTimeWeight));

            // Müşterinin öncelik skorunu güncelle  
            customer.PriorityScore = existingOrder.OrderPriority;
        }

        _context.SaveChanges();

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
        existingOrder.OrderStatus = "Reddedildi";
        existingOrder.ApprovalDate = DateTime.Now; // Reddetme tarihi kaydedilir  
        existingOrder.WaitTime = existingOrder.ApprovalDate - existingOrder.OrderDate; // WaitTime'ı hesapla  
        // Değişiklikleri kaydet  
        _context.SaveChanges();

        return RedirectToAction("OrderList"); // Sipariş listesi sayfasına yönlendir  
    }
    public IActionResult CalculatePriorty(int customerId)//neden bu ikinciye eşit oldu
    {
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        if (customer != null)
        {

            // Temel öncelik skorunu belirle  
            int basePriority = customer.CustomerType == "Premium" ? 15 : 10;
            double waitTimeWeight = 0.5;
            
        
        }
        return RedirectToAction("OrderList");
    }
}