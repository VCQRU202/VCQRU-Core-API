using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Updateprofile : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Updateprofile(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsumer([FromBody] updateprofilerequest req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Consumer data is null."));

            try
            {
                var consumerData = req.Request.Split("<@>", StringSplitOptions.None);
                var consumer = new Consumer();
                foreach (var data in consumerData)
                {
                    var keyValue = data.Split("=");
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();

                        if(key== "Vrkabel_User_Type")
                        {
                            string qry = $"select top 1 Row_ID from User_Type where User_Type='{value}' and Comp_ID='{req.Comp_id}'";
                            var dtuserid= await  _databaseManager.ExecuteDataTableAsync(qry);
                            if (dtuserid.Rows.Count > 0)
                            {
                                value = dtuserid.Rows[0]["Row_ID"].ToString();
                            }
                        }

                        switch (key)
                        {
                            case "MobileNo":
                                consumer.Mobile = value;
                                break;
                            case "User_ID":
                                consumer.User_ID = value;
                                break;
                            case "ConsumerName":
                                consumer.ConsumerName = value;
                                break;
                            case "Email":
                                consumer.Email = value;
                                break;
                            case "Mobile":
                                consumer.Mobile = value;
                                break;
                            case "City":
                                consumer.City = value;
                                break;
                            case "PinCode":
                                consumer.PinCode = value;
                                break;
                            case "Password":
                                consumer.Password = value;
                                break;
                            case "IsActive":
                                consumer.IsActive = Convert.ToInt32(value);
                                break;
                            case "IsDelete":
                                consumer.IsDelete = Convert.ToInt32(value);
                                break;
                            case "Address":
                                consumer.Address = value;
                                break;
                            case "Per_Address":
                                consumer.Per_Address = value;
                                break;
                            case "ReferralCode":
                                consumer.ReferralCode = Convert.ToInt32(value);
                                break;
                            case "IsSharedReferralCode":
                                consumer.IsSharedReferralCode = Convert.ToBoolean(value);
                                break;
                            case "employeeID":
                                consumer.employeeID = value;
                                break;
                            case "distributorID":
                                consumer.distributorID = value;
                                break;
                            case "aadharNumber":
                                consumer.aadharNumber = value;
                                break;
                            case "aadharFile":
                                consumer.aadharFile = value;
                                break;
                            case "aadharback":
                                consumer.aadharback = value;
                                break;
                            case "aadharUploadedBy":
                                consumer.aadharUploadedBy = value;
                                break;
                            case "Aadhar_source":
                                consumer.Aadhar_source = value;
                                break;
                            case "village":
                                consumer.village = value;
                                break;
                            case "district":
                                consumer.district = value;
                                break;
                            case "State":
                                consumer.state = value;
                                break;
                            case "country":
                                consumer.country = value;
                                break;
                            case "Role_Id":
                                consumer.Role_Id = Convert.ToInt32(value);
                                break;
                            case "Created_by":
                                consumer.Created_by = Convert.ToInt32(value);
                                break;
                            case "Comp_id":
                                consumer.Comp_id = value;
                                break;
                            case "SellerName":
                                consumer.SellerName = value;
                                break;
                            case "token":
                                consumer.token = value;
                                break;
                            case "MStarId":
                                consumer.MStarId = Convert.ToInt32(value);
                                break;
                            case "Inox_User_Type":
                                consumer.Inox_User_Type = value;
                                break;
                            case "Vrkabel_User_Type":
                                consumer.Vrkabel_User_Type = Convert.ToInt32(value);
                                break;
                            case "cin_number":
                                consumer.cin_number = value;
                                break;
                            case "ref_cin_number":
                                consumer.ref_cin_number = value;
                                break;
                            case "designation":
                                consumer.designation = value;
                                break;
                            case "dob":
                                consumer.dob = value;
                                break;
                            case "Gender":
                                consumer.gender = value;
                                break;
                            case "sur_name":
                                consumer.sur_name = value;
                                break;
                            case "communication_status":
                                consumer.communication_status = Convert.ToInt32(value);
                                break;
                            case "business_status":
                                consumer.business_status = Convert.ToInt32(value);
                                break;
                            case "house_number":
                                consumer.house_number = value;
                                break;
                            case "land_mark":
                                consumer.land_mark = value;
                                break;
                            case "owner_number":
                                consumer.owner_number = value;
                                break;
                            case "shop_name":
                                consumer.shop_name = value;
                                break;
                            case "pancard_number":
                                consumer.pancard_number = value;
                                break;
                            case "gst_number":
                                consumer.gst_number = value;
                                break;
                            case "pan_card_file":
                                consumer.pan_card_file = value;
                                break;
                            case "shop_file":
                                consumer.shop_file = value;
                                break;
                            case "Other_Role":
                                consumer.Other_Role = value;
                                break;
                            case "profile_image":
                                consumer.profile_image = value;
                                break;
                            case "VRKbl_KYC_status":
                                consumer.VRKbl_KYC_status = Convert.ToInt32(value);
                                break;
                            case "Additional":
                                consumer.Additional = value;
                                break;
                            case "remark":
                                consumer.remark = value;
                                break;
                            case "panekycStatus":
                                consumer.panekycStatus = value;
                                break;
                            case "aadharkycStatus":
                                consumer.aadharkycStatus = value;
                                break;
                            case "bankekycStatus":
                                consumer.bankekycStatus = value;
                                break;
                            case "PanHolderName":
                                consumer.PanHolderName = value;
                                break;
                            case "AadharHolderName":
                                consumer.AadharHolderName = value;
                                break;
                            case "Shop_address":
                                consumer.Shop_address = value;
                                break;
                            case "FirmName":
                                consumer.FirmName = value;
                                break;
                            case "Apptoken":
                                consumer.Apptoken = value;
                                break;
                            case "AppVersion":
                                consumer.AppVersion = value;
                                break;
                            case "Agegroup":
                                consumer.Agegroup = value;
                                break;
                            case "Pancard_Status":
                                consumer.Pancard_Status = value;
                                break;
                            case "BrandId":
                                consumer.BrandId = Convert.ToInt32(value);
                                break;
                            case "Aadhar_Status":
                                consumer.Aadhar_Status = value;
                                break;
                            case "Passbook_Status":
                                consumer.Passbook_Status = value;
                                break;
                            case "Ekyc_status":
                                consumer.Ekyc_status = value;
                                break;
                            case "Location":
                                consumer.Location = value;
                                break;
                            case "User_Type":
                                consumer.User_Type = Convert.ToInt32(value);
                                break;
                            case "AddressProof":
                                consumer.AddressProof = value;
                                break;
                            case "UpiidImage":
                                consumer.UpiidImage = value;
                                break;
                            case "UPIKYCSTATUS":
                                consumer.UPIKYCSTATUS = Convert.ToInt32(value);
                                break;
                            case "teslapayoutmode":
                                consumer.teslapayoutmode = value;
                                break;
                            case "Selfie_image":
                                consumer.Selfie_image = value;
                                break;
                            default:
                                break;
                        }
                    }
                }


                if (consumer.Mobile.Length == 10)
                    consumer.Mobile = "91" + consumer.Mobile;

                string Querystring = $"select * from M_Consumer where MobileNo='{consumer.Mobile}' and IsDelete=0";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(Querystring);

                if (dtconsu.Rows.Count > 0)
                {
                    Dictionary<string, object> inputParametersupdate = new Dictionary<string, object>
                    {
                        { "@ConsumerName", consumer.ConsumerName },
                        { "@Email", consumer.Email },
                        { "@MobileNo", consumer.Mobile },
                        { "@City", consumer.City },
                        { "@PinCode", consumer.PinCode },
                        { "@Address", consumer.Address },
                        { "@employeeID", consumer.employeeID ?? (object)DBNull.Value },
                        { "@distributorID", consumer.distributorID ?? (object)DBNull.Value },
                        { "@aadharNumber", consumer.aadharNumber ?? (object)DBNull.Value },
                        { "@aadharFile", consumer.aadharFile ?? (object)DBNull.Value },
                        { "@aadharFile_back", consumer.aadharback ?? (object)DBNull.Value },
                        { "@uploadedby", consumer.aadharUploadedBy ?? (object)DBNull.Value },
                        { "@uploadedsource", consumer.Aadhar_source ?? (object)DBNull.Value },
                        { "@village", consumer.village ?? (object)DBNull.Value },
                        { "@district", consumer.district ?? (object)DBNull.Value },
                        { "@state", consumer.state ?? (object)DBNull.Value },
                        { "@country", consumer.country ?? (object)DBNull.Value },
                        { "@role_id", consumer.Role_Id },
                        { "@Comp_id", consumer.Comp_id ?? (object)DBNull.Value },
                        { "@Created_by", consumer.Created_by  },
                        { "@permanemt", consumer.Per_Address ?? (object)DBNull.Value },
                        { "@SellerName", consumer.SellerName ?? (object)DBNull.Value },
                        { "@Commision", consumer.communication_status  },
                        { "@token", consumer.token ?? (object)DBNull.Value },
                        { "@Inox_User_Type", consumer.Inox_User_Type ?? (object)DBNull.Value },
                        { "@Vrkabel_User_Type", consumer.Vrkabel_User_Type },
                        { "@Vr_cin_number", consumer.cin_number ?? (object)DBNull.Value },
                        { "@Vr_ref_cin_number", consumer.ref_cin_number ?? (object)DBNull.Value },
                        { "@Vr_designation", consumer.designation ?? (object)DBNull.Value },
                        { "@Vr_dob", consumer.dob ?? (object)DBNull.Value },
                        { "@Vr_gender", consumer.gender ?? (object)DBNull.Value },
                        { "@Vr_sur_name", consumer.sur_name ?? (object)DBNull.Value },
                        { "@Vr_communication_status", consumer.communication_status },
                        { "@Vr_business_status", consumer.communication_status },
                        { "@Vr_house_number", consumer.communication_status },
                        { "@Vr_land_mark", consumer.communication_status },
                        { "@Other_Role", consumer.Other_Role ?? (object)DBNull.Value },
                        { "@UPI", consumer.upi ?? (object)DBNull.Value },
                        { "@gst_number", consumer.gst_number ?? (object)DBNull.Value },
                        { "@shop_file", consumer.shop_file ?? (object)DBNull.Value },
                        { "@shop_name", consumer.shop_name ?? (object)DBNull.Value },
                        { "@Shop_address", consumer.Shop_address ?? (object)DBNull.Value },
                        { "@FirmName", consumer.FirmName ?? (object)DBNull.Value },
                        { "@pancard_number", consumer.pancard_number ?? (object)DBNull.Value },
                        { "@pan_card_file", consumer.pan_card_file ?? (object)DBNull.Value },
                        { "@Agegroup", consumer. Agegroup ?? (object)DBNull.Value },
                        { "@ReferralCode", consumer.ReferralCode },
                        { "@teslapayoutmode", consumer.teslapayoutmode ?? (object)DBNull.Value },
                        { "@M_ConsumerId", req.M_Consumerid }
                    };
                    var resultdata = await _databaseManager.ExecuteStoredProcedureDataSetAsync("USP_UPDATECONSUMERDATA", inputParametersupdate);
                    string Querystring1 = $"select * from M_Consumer where MobileNo='{consumer.Mobile}' and IsDelete=0";
                    var dtconsu1 = await _databaseManager.ExecuteDataTableAsync(Querystring);
                    if (dtconsu1.Rows.Count > 0)
                    {
                        DataRow row = dtconsu1.Rows[0];
                        // Map the retrieved data to User_Details
                        var response = new
                        {
                            M_consumerid = Convert.ToInt32(row["M_Consumerid"]),
                            User_ID = row["User_ID"]?.ToString(),
                            ConsumerName = row["ConsumerName"]?.ToString(),
                            Email = row["Email"]?.ToString(),
                            MobileNo = row["MobileNo"]?.ToString(),
                            City = row["City"]?.ToString(),
                            state = row["state"]?.ToString(),
                            PinCode = row["PinCode"]?.ToString(),
                            Address = row["Address"]?.ToString(),
                            aadharNumber = row["aadharNumber"]?.ToString(),
                            district = row["district"]?.ToString(),
                            country = row["country"]?.ToString(),
                            Vrkabel_User_Type = row["Vrkabel_User_Type"]?.ToString(),
                            dob = row["dob"]?.ToString(),
                            gender = row["gender"]?.ToString(),
                        };
                        return Ok(new ApiResponse<object>(true, "Consumer profile updated successfully.", response));
                    }
                    return Ok(new ApiResponse<object>(true, "Consumer profile updated successfully.", consumer));
                }
                else
                {
                    return Conflict(new ApiResponse<object>(false, "Invalid Consumer Details."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Profile Update API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, $"Internal Server Error: {ex.Message}"));
            }
        }
    }

    public class updateprofilerequest
    {
        public string Request { get; set; }
        public string? M_Consumerid { get; set; }
        public string? Comp_id { get; set; }
    }
}
