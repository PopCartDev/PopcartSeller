namespace PopcartSeller.Models
{
    public class ProductResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public GetProduct Data { get; set; }
    }

    public class GetProduct
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public Seller Seller { get; set; }
        public Category Category { get; set; }
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

    public class Seller
    {
        public string _id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string UserType { get; set; }
    }

    public class Category
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
