namespace PopcartSeller.Models;

 public class Product
 {
        public string category { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string brand { get; set; }
        public string price { get; set; }
        public List<IFormFile> images { get; set; } 
 }
