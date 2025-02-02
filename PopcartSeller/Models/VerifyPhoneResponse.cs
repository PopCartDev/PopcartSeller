namespace PopcartSeller.Models;

 public class VerifyPhoneResponse
 {
        public bool Status { get; set; }
        public string Message { get; set; }
        public VerifyPhoneData Data { get; set; }
    

    public class VerifyPhoneData
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
 }
