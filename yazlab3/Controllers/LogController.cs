using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Diagnostics;
using yazlab3.Models;

namespace yazlab3.Controllers.LogController
{
    public enum UserType
    {
        Admin = 1,
        Musteri = 2
    }
    public class Log
    {
        private LogContext _LogContextObj;
        public Log(int? id, UserType? type, int? order_id, string? title, string? log)
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
