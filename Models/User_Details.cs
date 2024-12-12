using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoreApi_BL_App.Models
{
    public class User_Details
    {
        public string M_Consumerid { get; set; }
        public string User_ID { get; set; }
        public string ConsumerName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public string Password { get; set; }
        public int IsActive { get; set; }
        public int IsDelete { get; set; }
        public string DML { get; set; }
        public string Address { get; set; }
        public DateTime Entry_Date { get; set; }
        public string code1 { get; set; }
        public string code2 { get; set; }
        public string MMEmployeID { get; set; }
        public string MMDistributorID { get; set; }
        public string aadharNumber { get; set; }
        public string village { get; set; }
        public string district { get; set; }
        public string state { get; set; }
        public string Comp_id { get; set; }
        public int role_id { get; set; }
        public string country { get; set; }
        public string aadharFile { get; set; }
        public string aadharback { get; set; }
        public string SellerName { get; set; }
        public string uploadedby { get; set; }
        public string uploadedsource { get; set; }

        public int Vrkabel_User_Type { get; set; }
        public int User_Type { get; set; }

        public string distributorID { get; set; }
        public string employeeID { get; set; }

        public string Profile_image { get; set; }
        public string Shopname { get; set; }
        public string GstNo { get; set; }
        public string Shop_image { get; set; }
        
        public string ShopAddess { get; set; }
        public string FirmName { get; set; }
        public string BrandId { get; set; }

        public string ReferralCode { get; set; }

        public string UpiId { get; set; } /* For Hannover*/
        public string pancard_number { get; set; } /* For oci Tej*/
        public string teslapayoutmode { get; set; } /* For oci Tej*/
    }

  
}