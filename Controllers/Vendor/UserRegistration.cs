﻿using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using System.Collections.Generic;

namespace CoreApi_BL_App.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistration : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public UserRegistration(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsumer([FromBody] consumerrequest req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Consumer data is null."));

            try
            {
                // Step 1: Parse the request data (extract fields dynamically)
                var consumerData = req.Request.Split("<@>", StringSplitOptions.None);

                var consumer = new Consumer();

                foreach (var data in consumerData)
                {
                    var keyValue = data.Split("=");
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim();
                        var value = keyValue[1].Trim();
                        if (key == "Vrkabel_User_Type")
                        {
                            string qry = $"select top 1 Row_ID from User_Type where User_Type='{value}' and Comp_ID='{req.Comp_id}'";
                            var dtuserid = await _databaseManager.ExecuteDataTableAsync(qry);
                            if (dtuserid.Rows.Count > 0)
                            {
                                value = dtuserid.Rows[0]["Row_ID"].ToString();
                            }
                        }

                        // Assign values to consumer object dynamically
                        switch (key)
                        {
                            case "User_ID":
                                consumer.User_ID = value;
                                break;
                            case "ConsumerName":
                                consumer.ConsumerName = value;
                                break;
                            case "Email":
                                consumer.Email = value;
                                break;
                            case "MobileNo":
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
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.IsActive = Convert.ToInt32(value);
                                break;
                            case "IsDelete":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.IsDelete = Convert.ToInt32(value);
                                break;
                            case "Address":
                                consumer.Address = value;
                                break;
                            case "Per_Address":
                                consumer.Per_Address = value;
                                break;
                            case "ReferralCode":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.ReferralCode = Convert.ToInt32(value);
                                break;
                            case "IsSharedReferralCode":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                            case "state":
                                consumer.state = value;
                                break;
                            case "country":
                                consumer.country = value;
                                break;
                            case "Role_Id":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.Role_Id = Convert.ToInt32(value);
                                break;
                            case "Created_by":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.MStarId = Convert.ToInt32(value);
                                break;
                            case "Inox_User_Type":
                                consumer.Inox_User_Type = value;
                                break;
                            case "Vrkabel_User_Type":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                            case "gender":
                                consumer.gender = value;
                                break;
                            case "sur_name":
                                consumer.sur_name = value;
                                break;
                            case "communication_status":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.communication_status = Convert.ToInt32(value);
                                break;
                            case "business_status":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
                                consumer.User_Type = Convert.ToInt32(value);
                                break;
                            case "AddressProof":
                                consumer.AddressProof = value;
                                break;
                            case "UpiidImage":
                                consumer.UpiidImage = value;
                                break;
                            case "UPIKYCSTATUS":
                                if (string.IsNullOrEmpty(value))
                                {
                                    value = "0";
                                }
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

                // Ensure the mobile number has the correct prefix if necessary
                if (consumer.Mobile.Length == 10)
                    consumer.Mobile = "91" + consumer.Mobile;

                // Check if the user already exists based on mobile number
                string Querystring = $"select * from M_Consumer where MobileNo='{consumer.Mobile}' and IsDelete=0";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(Querystring);

                if (dtconsu.Rows.Count == 0)
                {
                    // Insert a new consumer
                    Dictionary<string, object> inputParameters = new Dictionary<string, object>
                    {
                        { "@MobileNo", consumer.Mobile },
                        { "@Password", "12345" }, // You may need to generate a random password or send a default one
                        { "@Entry_Date", DBNull.Value },
                        { "@IsActive", true },
                        { "@IsDelete", false }
                    };
                    List<string> outputParameters = new List<string> { "@User_ID", "@M_Consumerid" };
                    Dictionary<string, object> result = _databaseManager.ExecuteStoredProcedure("USP_Consumerreg", inputParameters, outputParameters);
                    string userId = result["@User_ID"].ToString();
                    string consumerId = result["@M_Consumerid"].ToString();
                    consumer.M_Consumerid =Convert.ToInt32( consumerId);
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
                        { "@M_ConsumerId", consumer.M_Consumerid }
                    };


                    var resultdata = await _databaseManager.ExecuteStoredProcedureDataSetAsync("USP_UPDATECONSUMERDATA", inputParametersupdate);



                    return Ok(new ApiResponse<object>(true, "Consumer registered successfully.", new
                    {
                        UserId = userId,
                        M_Consumerid = consumerId,
                        MobileNo = consumer.Mobile,
                        ConsumerName = consumer.ConsumerName
                    }));
                }
                else
                {
                    return Conflict(new ApiResponse<object>(false, "Consumer already exists."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in UserRegistration API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, $"Internal Server Error: {ex.Message}"));
            }
        }
    }
}
