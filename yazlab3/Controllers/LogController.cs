using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using yazlab3.Models;

namespace yazlab3.Controllers.LogController
{
    public enum UserType
    {
        Admin = 1,
        Customer = 2
    }
    public class Log
    {
        private LogContext _LogContextObj;
        public Log(int? id, int? order_id, UserType? type, string? title, string? log)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LogContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-PMBMQKM\\SQLEXPRESS;Database=yazlab3;Integrated Security=True;TrustServerCertificate=True;");

            _LogContextObj = new LogContext(optionsBuilder.Options);
            if (id == -1 || id == null)
                return;

            var newLog = new yazlab3.Models.Log
            {
                CustomerID = id,
                OrderID = order_id,
                LogDate = DateTime.Now,
                LogType = type + title,
                LogDetails = log
            };
            Task.Run(async () =>
            {
                try
                {
                    await LogEkleAsync(newLog);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Hata:" + ex.Message);
                }
            });
        }
        public async Task<(string CustomerName, string ProductName, int Quantity)?> GetCustomerAndOrderDetails(int customerId, int orderId)
        {
            try
            {
                Debug.WriteLine($"Sorgu çalıştırılıyor: GetCustomerAndOrderDetails, CustomerID: {customerId}, OrderID: {orderId}");

                var logData = await _LogContextObj.Logs
                    .Where(log => log.CustomerID == customerId && log.OrderID == orderId)
                    .Join(_LogContextObj.Customers,
                     log => log.CustomerID,
                     customer => customer.CustomerID,
                     (log, customer) => new { log, customer })
                    .Join(_LogContextObj.Orders,
                     joinResult => joinResult.log.OrderID,
                     order => order.OrderID,
                     (joinResult, order) => new { joinResult.customer.CustomerName, order.ProductID, order.Quantity })
                    .Join(_LogContextObj.Products,
                      joinResult => joinResult.ProductID,
                      product => product.ProductID,
                      (joinResult, product) => new { joinResult.CustomerName, product.ProductName, joinResult.Quantity })
                    .FirstOrDefaultAsync();

                if (logData != null)
                {
                    Debug.WriteLine($"Sorgu sonucu: Müşteri Adı: {logData.CustomerName}, Ürün Adı: {logData.ProductName}, Miktar: {logData.Quantity}");
                    return (logData.CustomerName, logData.ProductName, logData.Quantity);
                }
                Debug.WriteLine("Sorgu sonucu: Belirtilen log kaydına ait müşteri ve ürün bilgisi bulunamadı.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sorgu hatası: {ex.Message}");
                return null;
            }
        }
        public async Task LogEkleAsync(Models.Log log)
        {
            try
            {
                _LogContextObj.Logs.Add(log);
                await _LogContextObj.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HataXXX: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
            }


        }
    }
}