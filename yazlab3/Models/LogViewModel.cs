// Models klasörünüzün içine veya uygun bir yere ekleyebilirsiniz.
using System;

namespace yazlab3.Models
{
    public class LogViewModel
    {
        public int LogID { get; set; }
        public int? CustomerID { get; set; }
        public int? OrderID { get; set; }
        public string LogType { get; set; }
        public string LogDetails { get; set; }
        public DateTime LogDate { get; set; }
        public string? CustomerType { get; set; }
        public string? ProductName { get; set; }
        public int? Quantity { get; set; }
        public string? ProcessResult { get; set; }
    }
}