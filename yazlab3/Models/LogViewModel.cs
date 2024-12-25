namespace yazlab3.ViewModels
{
    public class LogViewModel
    {
        public int LogID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerType { get; set; }
        public int OrderID { get; set; }
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
        public string LogType { get; set; }
        public string LogDetails { get; set; }
        public DateTime LogDate { get; set; }
    }
}
