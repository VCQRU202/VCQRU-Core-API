using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Profiledetails : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Profiledetails(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetFieldSettings([FromBody] profiledetailsrequest Req)
        {
            string Profiledata = "";
            string kycdata = "";
            if (Req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            if (string.IsNullOrEmpty(Req.Mobileno) || string.IsNullOrEmpty(Req.Comp_id))
                return BadRequest(new ApiResponse<object>(false, "Mobile number or Company ID is missing."));

            try
            {
                if (Req.Mobileno.Length == 10)
                    Req.Mobileno = "91" + Req.Mobileno;

                if (Req.Mobileno.Length != 12)
                    return BadRequest(new ApiResponse<object>(false, "Invalid Mobile Number"));

                string consqry = "exec USP_PROFILEDEATILS_BL @Mobileno, @Comp_id";
                var parameters = new Dictionary<string, object>
                    {
                        { "@Mobileno", Req.Mobileno },
                        { "@Comp_id", Req.Comp_id }
                    };

                var result = await _databaseManager.ExecuteDataTableAsync(consqry, parameters);
                string jsonqry = $"Select profilesettings,kyc_Details from BrandSettings where comp_id='{Req.Comp_id}'";
                var resultprofile = await _databaseManager.ExecuteDataTableAsync(jsonqry);
                if (resultprofile.Rows.Count > 0)
                {
                    Profiledata = resultprofile.Rows[0]["profilesettings"].ToString();
                    kycdata = resultprofile.Rows[0]["kyc_Details"].ToString();
                }
                if (result != null && result.Rows.Count > 0)
                {
                    var profileData = MapDataToProfile(result.Rows[0], Profiledata, kycdata);
                    return Ok(new ApiResponse<object>(true, "Data Retrieved Successfully", profileData));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid User Details"));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in Profiledetails API: " + ex.Message + "  ,Track and Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
            }
        }

        private object MapDataToProfile(DataRow row, string profiledata, string kycdata)
        {
            if (row == null) return null;


            var kycFields = ParseDynamicData(kycdata);
            var profileFields = ParseDynamicDataprofile(profiledata);
            int kycFilledCount = kycFields.Count(f => f.Value == "Yes");
            int kyccount = 0;
            var fieldNameMapping = new Dictionary<string, string>
                {
                    { "AadharCard", "aadharkycStatus" },
                    { "PANCard", "panekycStatus" },
                    { "AccountDetails", "bankekycStatus" },
                    { "UPI", "UPIKYCSTATUS" }
                };
            foreach (var field in kycFields)
            {
                var kycFieldName = field.Key;
                if (fieldNameMapping.ContainsKey(kycFieldName))
                {
                    kycFieldName = fieldNameMapping[kycFieldName];
                }
                if (kycFieldName.Contains("Codecheck") || kycFieldName.Contains("claimrequired"))
                {
                    continue; 
                }
                var fieldValue = row[kycFieldName]?.ToString();
                if (fieldValue == "Online" || fieldValue == "1")
                {
                    kyccount++;
                }

            }

            double kycPercentage = kycFilledCount > 0 ? (kyccount / (double)kycFilledCount) * 100 : 0;
            int profileFilledCount = 0;
            int profileTotalFields = profileFields.Count;


            foreach (var field in profileFields)
            {
                var profileFieldName = field;
                var fieldValue = row[profileFieldName]?.ToString();

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    profileFilledCount++;
                }
            }

            double profilePercentage = profileTotalFields > 0 ? (profileFilledCount / (double)profileTotalFields) * 100 : 0;
            double finalPercentage = (kycPercentage + profilePercentage) / 2;


            var profileData = new ProfileDetails
            {
                M_Consumerid = row["M_Consumerid"]?.ToString(),
                User_ID = row["User_ID"]?.ToString(),
                ConsumerName = row["ConsumerName"]?.ToString(),
                Email = row["Email"]?.ToString(),
                MobileNo = row["MobileNo"]?.ToString(),
                City = row["City"]?.ToString(),
                PinCode = row["PinCode"]?.ToString(),
                Entry_Date = row["Entry_Date"]?.ToString(),
                IsActive = row["IsActive"] != DBNull.Value ? Convert.ToBoolean(row["IsActive"]) : false,
                IsDeleted = row["IsDeleted"] != DBNull.Value ? Convert.ToBoolean(row["IsDeleted"]) : false,
                Address = row["Address"]?.ToString(),
                Per_Address = row["Per_Address"]?.ToString(),
                ReferralCode = row["ReferralCode"]?.ToString(),
                IsSharedReferralCode = row["IsSharedReferralCode"] != DBNull.Value ? Convert.ToBoolean(row["IsSharedReferralCode"]) : false,
                EmployeeID = row["employeeID"]?.ToString(),
                DistributorID = row["distributorID"]?.ToString(),
                AadharNumber = row["aadharNumber"]?.ToString(),
                AadharFile = row["aadharFile"]?.ToString(),
                AadharBack = row["aadharback"]?.ToString(),
                AadharUploadDate = row["aadharUploadedate"]?.ToString(),
                AadharUploadedBy = row["aadharUploadedBy"]?.ToString(),
                AadharSource = row["Aadhar_source"]?.ToString(),
                Village = row["village"]?.ToString(),
                District = row["district"]?.ToString(),
                State = row["state"]?.ToString(),
                Country = row["country"]?.ToString(),
                Role_Id = row["Role_Id"]?.ToString(),
                CreatedBy = row["Created_by"]?.ToString(),
                Comp_id = row["Comp_id"]?.ToString(),
                SellerName = row["SellerName"]?.ToString(),
                Token = row["token"]?.ToString(),
                MStarId = row["MStarId"]?.ToString(),
                Inox_User_Type = row["Inox_User_Type"]?.ToString(),
                Vrkabel_User_Type = row["Vrkabel_User_Type"]?.ToString(),
                CinNumber = row["cin_number"]?.ToString(),
                RefCinNumber = row["ref_cin_number"]?.ToString(),
                Designation = row["designation"]?.ToString(),
                Dob = row["dob"]?.ToString(),
                Gender = row["gender"]?.ToString(),
                SurName = row["sur_name"]?.ToString(),
                CommunicationStatus = row["communication_status"]?.ToString(),
                BusinessStatus = row["business_status"]?.ToString(),
                HouseNumber = row["house_number"]?.ToString(),
                LandMark = row["land_mark"]?.ToString(),
                OwnerNumber = row["owner_number"]?.ToString(),
                ShopName = row["shop_name"]?.ToString(),
                PancardNumber = row["pancard_number"]?.ToString(),
                GstNumber = row["gst_number"]?.ToString(),
                PanCardFile = row["pan_card_file"]?.ToString(),
                ShopFile = row["shop_file"]?.ToString(),
                OtherRole = row["Other_Role"]?.ToString(),
                ProfileImage = row["profile_image"]?.ToString(),
                VRKblKYCStatus = row["VRKbl_KYC_status"]?.ToString(),
                Additional = row["Additional"]?.ToString(),
                Remark = row["remark"]?.ToString(),
                PanekycStatus = row["panekycStatus"]?.ToString(),
                AadharKycStatus = row["aadharkycStatus"]?.ToString(),
                BankKycStatus = row["bankekycStatus"]?.ToString(),
                PanHolderName = row["PanHolderName"]?.ToString(),
                AadharHolderName = row["AadharHolderName"]?.ToString(),
                UPIId = row["UPIId"]?.ToString(),
                ShopAddress = row["Shop_address"]?.ToString(),
                FirmName = row["FirmName"]?.ToString(),
                Apptoken = row["Apptoken"]?.ToString(),
                AppVersion = row["AppVersion"]?.ToString(),
                Agegroup = row["Agegroup"]?.ToString(),
                PancardStatus = row["Pancard_Status"]?.ToString(),
                BrandId = row["BrandId"]?.ToString(),
                AadharStatus = row["Aadhar_Status"]?.ToString(),
                PassbookStatus = row["Passbook_Status"]?.ToString(),
                EkycStatus = row["Ekyc_status"]?.ToString(),
                Location = row["Location"]?.ToString(),
                UserType = row["User_Type"]?.ToString(),
                AddressProof = row["AddressProof"]?.ToString(),
                UpiidImage = row["UpiidImage"]?.ToString(),
                UPIKYCStatus = row["UPIKYCSTATUS"]?.ToString(),
                TeslaPayoutMode = row["teslapayoutmode"]?.ToString(),
                SelfieImage = row["Selfie_image"]?.ToString(),
                UserRoleType = row["User Role Type"]?.ToString(),
                Persent = finalPercentage.ToString("0")
            };
            return profileData;
        }

        private Dictionary<string, string> ParseDynamicData(string data)
        {
            var parsedData = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(data))
                return parsedData;

            try
            {
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

                foreach (var entry in jsonData)
                {
                    parsedData[entry.Key] = entry.Value?.ToString();
                }
            }
            catch (JsonException ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Profiledetails API  ParseDynamicData method :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
            }
            return parsedData;
        }

        public List<string> ParseDynamicDataprofile(string data)
        {
            var fieldNames = new List<string>();

            if (string.IsNullOrEmpty(data))
                return fieldNames;

            try
            {
                var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data);

                foreach (var entry in jsonData)
                {
                    if (entry.ContainsKey("FieldName"))
                    {
                        string fieldName = entry["FieldName"]?.ToString();
                        fieldNames.Add(fieldName);
                    }
                }
            }
            catch (JsonException ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Profiledetails API  ParseDynamicDataprofile method :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
            }
            return fieldNames;
        }
    }

    public class profiledetailsrequest
    {
        public string Comp_id { get; set; }
        public string Mobileno { get; set; }
        public string M_consumerid { get; set; }
    }

    public class ProfileDetails
    {
        public string? M_Consumerid { get; set; }
        public string? User_ID { get; set; }
        public string? ConsumerName { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? City { get; set; }
        public string? PinCode { get; set; }
        public string? Entry_Date { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string? Address { get; set; }
        public string? Per_Address { get; set; }
        public string? ReferralCode { get; set; }
        public bool IsSharedReferralCode { get; set; }
        public string? EmployeeID { get; set; }
        public string? DistributorID { get; set; }
        public string? AadharNumber { get; set; }
        public string? AadharFile { get; set; }
        public string? AadharBack { get; set; }
        public string? AadharUploadDate { get; set; }
        public string? AadharUploadedBy { get; set; }
        public string? AadharSource { get; set; }
        public string? Village { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Role_Id { get; set; }
        public string? CreatedBy { get; set; }
        public string? Comp_id { get; set; }
        public string? SellerName { get; set; }
        public string? Token { get; set; }
        public string? MStarId { get; set; }
        public string? Inox_User_Type { get; set; }
        public string? Vrkabel_User_Type { get; set; }
        public string? CinNumber { get; set; }
        public string? RefCinNumber { get; set; }
        public string? Designation { get; set; }
        public string? Dob { get; set; }
        public string? Gender { get; set; }
        public string? SurName { get; set; }
        public string? CommunicationStatus { get; set; }
        public string? BusinessStatus { get; set; }
        public string? HouseNumber { get; set; }
        public string? LandMark { get; set; }
        public string? OwnerNumber { get; set; }
        public string? ShopName { get; set; }
        public string? PancardNumber { get; set; }
        public string? GstNumber { get; set; }
        public string? PanCardFile { get; set; }
        public string? ShopFile { get; set; }
        public string? OtherRole { get; set; }
        public string? ProfileImage { get; set; }
        public string? VRKblKYCStatus { get; set; }
        public string? Additional { get; set; }
        public string? Remark { get; set; }
        public string? PanekycStatus { get; set; }
        public string? AadharKycStatus { get; set; }
        public string? BankKycStatus { get; set; }
        public string? PanHolderName { get; set; }
        public string? AadharHolderName { get; set; }
        public string? UPIId { get; set; }
        public string? ShopAddress { get; set; }
        public string? FirmName { get; set; }
        public string? Apptoken { get; set; }
        public string? AppVersion { get; set; }
        public string? Agegroup { get; set; }
        public string? PancardStatus { get; set; }
        public string? BrandId { get; set; }
        public string? AadharStatus { get; set; }
        public string? PassbookStatus { get; set; }
        public string? EkycStatus { get; set; }
        public string? Location { get; set; }
        public string? UserType { get; set; }
        public string? AddressProof { get; set; }
        public string? UpiidImage { get; set; }
        public string? UPIKYCStatus { get; set; }
        public string? TeslaPayoutMode { get; set; }
        public string? SelfieImage { get; set; }
        public string? UserRoleType { get; set; }
        public string? Persent { get; set; }
    }
}
