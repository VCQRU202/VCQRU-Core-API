namespace CoreApi_BL_App.Models.Vendor
{
    public class E_KYCSelectOptions
    {
        public int ID { get; set; }
        public List<string> SelectedOptions { get; set; } = new List<string>();
    }

    public class EKYCResponse
    {
        public int ID { get; set; }
        public List<string> SelectedOptions { get; set; }
    }
}
