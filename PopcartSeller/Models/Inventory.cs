namespace PopcartSeller.Models
{
    public class InventoryResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public InventoryData Data { get; set; }
    }

    public class InventoryData
    {
        public List<Inventory> Products { get; set; }
    }

    public class Inventory
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string Seller { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public int StockUnit { get; set; }
        public List<string> Images { get; set; }
        public bool Available { get; set; }
        public bool Published { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
