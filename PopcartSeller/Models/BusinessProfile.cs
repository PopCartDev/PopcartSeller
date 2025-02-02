namespace PopcartSeller.Models;
public class BusinessProfile

{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public string? Greeting { get; set; }
    public Data data { get; set; }
    public string? Token { get; set; }
    

    public class Data
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? UserType { get; set; }
        public bool Active { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int V { get; set; }
        public BusinessProfile? businessProfile { get; set; }

        public class BusinessProfile
        {
            public string? Id { get; set; }
            public string? User { get; set; }
            public string? BusinessName { get; set; }
            public bool Registered { get; set; }
            public Identification? identification { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int V { get; set; }

            public class Identification
            {
                public string? Type { get; set; }
                public bool Verified { get; set; }
                public string? Image { get; set; }
            }
        }
    }
}
