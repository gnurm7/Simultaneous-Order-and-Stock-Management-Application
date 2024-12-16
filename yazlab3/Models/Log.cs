namespace yazlab3.Models
{
    public class Log
    {
        public int LogID { get; set; }
        public int? CustomerID { get; set; } // Nullable Foreign Key
        public int? OrderID { get; set; } // Nullable Foreign Key
        public DateTime LogDate { get; set; }
        public string LogType { get; set; } // e.g., "Error", "Warning", "Info"
        public string LogDetails { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public Order Order { get; set; }
    }
}