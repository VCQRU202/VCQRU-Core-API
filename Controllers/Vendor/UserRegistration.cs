using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Azure.Core;
namespace CoreApi_BL_App.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistration : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserRegistration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // Post: Insert vendor data into the database
        [HttpPost]
        public async Task<IActionResult> CreateConsumer([FromBody] consumerrequest req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Consumer data is null."));

            var connectionString = _configuration.GetConnectionString("defaultConnectionbeta");

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
                            case "state":
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
                            case "gender":
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
                                // Log or handle unknown keys if necessary
                                break;
                        }
                    }
                }

                // Step 2: Get the new User ID from the database
                string UserId;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Update the PrStart for 'Consumer' and get the new ID
                    string updateQuery = "UPDATE Code_Gen SET PrStart = cast(PrStart as bigint) + 1 WHERE Prfor = 'Consumer'";
                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    string selectQuery = "SELECT PrPrefix + CONVERT(varchar, (PrStart - 1)) FROM Code_Gen WHERE Prfor = 'Consumer'";
                    using (var command = new SqlCommand(selectQuery, connection))
                    {
                        UserId = (string)await command.ExecuteScalarAsync();
                    }
                }

                // Step 3: Insert new consumer data into the database
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string insertQuery = @"
                INSERT INTO M_Consumer 
                (
                    User_ID, ConsumerName, Email, MobileNo, City, PinCode, Password, IsActive, IsDelete, 
                    Address, Per_Address, ReferralCode, IsSharedReferralCode, employeeID, distributorID, aadharNumber, aadharFile, 
                    aadharback, aadharUploadedBy, Aadhar_source, village, district, state, country, Role_Id, 
                    Created_by, Comp_id, SellerName, token, MStarId, Inox_User_Type, Vrkabel_User_Type, cin_number, ref_cin_number, 
                    designation, dob, gender, sur_name, communication_status, business_status, house_number, land_mark, owner_number, 
                    shop_name, pancard_number, gst_number, pan_card_file, shop_file, Other_Role, profile_image, VRKbl_KYC_status, 
                    Additional, remark, panekycStatus, aadharkycStatus, bankekycStatus, PanHolderName, AadharHolderName, Shop_address, 
                    FirmName, Apptoken, AppVersion, Agegroup, Pancard_Status, BrandId, Aadhar_Status, Passbook_Status, Ekyc_status, 
                    Location, User_Type, AddressProof, UpiidImage, UPIKYCSTATUS, teslapayoutmode, Selfie_image
                )
                VALUES 
                (
                    @User_ID, @ConsumerName, @Email, @MobileNo, @City, @PinCode, @Password, @IsActive, @IsDelete, 
                    @Address, @Per_Address, @ReferralCode, @IsSharedReferralCode, @employeeID, @distributorID, @aadharNumber, 
                    @aadharFile, @aadharback, @aadharUploadedBy, @Aadhar_source, @village, @district, @state, 
                    @country, @Role_Id, @Created_by, @Comp_id, @SellerName, @token, @MStarId, @Inox_User_Type, @Vrkabel_User_Type, 
                    @cin_number, @ref_cin_number, @designation, @dob, @gender, @sur_name, @communication_status, @business_status, 
                    @house_number, @land_mark, @owner_number, @shop_name, @pancard_number, @gst_number, @pan_card_file, @shop_file, 
                    @Other_Role, @profile_image, @VRKbl_KYC_status, @Additional, @remark, @panekycStatus, @aadharkycStatus, 
                    @bankekycStatus, @PanHolderName, @AadharHolderName, @Shop_address, @FirmName, @Apptoken, @AppVersion, @Agegroup, 
                    @Pancard_Status, @BrandId, @Aadhar_Status, @Passbook_Status, @Ekyc_status, @Location, @User_Type, @AddressProof, 
                    @UpiidImage, @UPIKYCSTATUS, @teslapayoutmode, @Selfie_image
                );";

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        // Add parameters dynamically for each property
                        command.Parameters.Add(CreateSqlParameter("@User_ID", UserId));
                        command.Parameters.Add(CreateSqlParameter("@ConsumerName", consumer.ConsumerName));
                        command.Parameters.Add(CreateSqlParameter("@Email", consumer.Email));
                        command.Parameters.Add(CreateSqlParameter("@MobileNo", consumer.Mobile));
                        command.Parameters.Add(CreateSqlParameter("@City", consumer.City));
                        command.Parameters.Add(CreateSqlParameter("@PinCode", consumer.PinCode));
                        command.Parameters.Add(CreateSqlParameter("@Password", consumer.Password ?? "defaultpassword"));
                        command.Parameters.Add(CreateSqlParameter("@IsActive", consumer.IsActive));
                        command.Parameters.Add(CreateSqlParameter("@IsDelete", consumer.IsDelete));
                        command.Parameters.Add(CreateSqlParameter("@Address", consumer.Address));
                        command.Parameters.Add(CreateSqlParameter("@Per_Address", consumer.Per_Address));
                        command.Parameters.Add(CreateSqlParameter("@ReferralCode", consumer.ReferralCode));
                        command.Parameters.Add(CreateSqlParameter("@IsSharedReferralCode", consumer.IsSharedReferralCode));
                        command.Parameters.Add(CreateSqlParameter("@employeeID", consumer.employeeID));
                        command.Parameters.Add(CreateSqlParameter("@distributorID", consumer.distributorID));
                        command.Parameters.Add(CreateSqlParameter("@aadharNumber", consumer.aadharNumber));
                        command.Parameters.Add(CreateSqlParameter("@aadharFile", consumer.aadharFile));
                        command.Parameters.Add(CreateSqlParameter("@aadharback", consumer.aadharback));
                        command.Parameters.Add(CreateSqlParameter("@aadharUploadedBy", consumer.aadharUploadedBy));
                        command.Parameters.Add(CreateSqlParameter("@Aadhar_source", consumer.Aadhar_source));
                        command.Parameters.Add(CreateSqlParameter("@village", consumer.village));
                        command.Parameters.Add(CreateSqlParameter("@district", consumer.district));
                        command.Parameters.Add(CreateSqlParameter("@state", consumer.state));
                        command.Parameters.Add(CreateSqlParameter("@country", consumer.country));
                        command.Parameters.Add(CreateSqlParameter("@Role_Id", consumer.Role_Id));
                        command.Parameters.Add(CreateSqlParameter("@Created_by", consumer.Created_by));
                        command.Parameters.Add(CreateSqlParameter("@Comp_id", consumer.Comp_id));
                        command.Parameters.Add(CreateSqlParameter("@SellerName", consumer.SellerName));
                        command.Parameters.Add(CreateSqlParameter("@token", consumer.token));
                        command.Parameters.Add(CreateSqlParameter("@MStarId", consumer.MStarId));
                        command.Parameters.Add(CreateSqlParameter("@Inox_User_Type", consumer.Inox_User_Type));
                        command.Parameters.Add(CreateSqlParameter("@Vrkabel_User_Type", consumer.Vrkabel_User_Type));
                        command.Parameters.Add(CreateSqlParameter("@cin_number", consumer.cin_number));
                        command.Parameters.Add(CreateSqlParameter("@ref_cin_number", consumer.ref_cin_number));
                        command.Parameters.Add(CreateSqlParameter("@designation", consumer.designation));
                        command.Parameters.Add(CreateSqlParameter("@dob", consumer.dob));
                        command.Parameters.Add(CreateSqlParameter("@gender", consumer.gender));
                        command.Parameters.Add(CreateSqlParameter("@sur_name", consumer.sur_name));
                        command.Parameters.Add(CreateSqlParameter("@communication_status", consumer.communication_status));
                        command.Parameters.Add(CreateSqlParameter("@business_status", consumer.business_status));
                        command.Parameters.Add(CreateSqlParameter("@house_number", consumer.house_number));
                        command.Parameters.Add(CreateSqlParameter("@land_mark", consumer.land_mark));
                        command.Parameters.Add(CreateSqlParameter("@owner_number", consumer.owner_number));
                        command.Parameters.Add(CreateSqlParameter("@shop_name", consumer.shop_name));
                        command.Parameters.Add(CreateSqlParameter("@pancard_number", consumer.pancard_number));
                        command.Parameters.Add(CreateSqlParameter("@gst_number", consumer.gst_number));
                        command.Parameters.Add(CreateSqlParameter("@pan_card_file", consumer.pan_card_file));
                        command.Parameters.Add(CreateSqlParameter("@shop_file", consumer.shop_file));
                        command.Parameters.Add(CreateSqlParameter("@Other_Role", consumer.Other_Role));
                        command.Parameters.Add(CreateSqlParameter("@profile_image", consumer.profile_image));
                        command.Parameters.Add(CreateSqlParameter("@VRKbl_KYC_status", consumer.VRKbl_KYC_status));
                        command.Parameters.Add(CreateSqlParameter("@Additional", consumer.Additional));
                        command.Parameters.Add(CreateSqlParameter("@remark", consumer.remark));
                        command.Parameters.Add(CreateSqlParameter("@panekycStatus", consumer.panekycStatus));
                        command.Parameters.Add(CreateSqlParameter("@aadharkycStatus", consumer.aadharkycStatus));
                        command.Parameters.Add(CreateSqlParameter("@bankekycStatus", consumer.bankekycStatus));
                        command.Parameters.Add(CreateSqlParameter("@PanHolderName", consumer.PanHolderName));
                        command.Parameters.Add(CreateSqlParameter("@AadharHolderName", consumer.AadharHolderName));
                        command.Parameters.Add(CreateSqlParameter("@Shop_address", consumer.Shop_address));
                        command.Parameters.Add(CreateSqlParameter("@FirmName", consumer.FirmName));
                        command.Parameters.Add(CreateSqlParameter("@Apptoken", consumer.Apptoken));
                        command.Parameters.Add(CreateSqlParameter("@AppVersion", consumer.AppVersion));
                        command.Parameters.Add(CreateSqlParameter("@Agegroup", consumer.Agegroup));
                        command.Parameters.Add(CreateSqlParameter("@Pancard_Status", consumer.Pancard_Status));
                        command.Parameters.Add(CreateSqlParameter("@BrandId", consumer.BrandId));
                        command.Parameters.Add(CreateSqlParameter("@Aadhar_Status", consumer.Aadhar_Status));
                        command.Parameters.Add(CreateSqlParameter("@Passbook_Status", consumer.Passbook_Status));
                        command.Parameters.Add(CreateSqlParameter("@Ekyc_status", consumer.Ekyc_status));
                        command.Parameters.Add(CreateSqlParameter("@Location", consumer.Location));
                        command.Parameters.Add(CreateSqlParameter("@User_Type", consumer.User_Type));
                        command.Parameters.Add(CreateSqlParameter("@AddressProof", consumer.AddressProof));
                        command.Parameters.Add(CreateSqlParameter("@UpiidImage", consumer.UpiidImage));
                        command.Parameters.Add(CreateSqlParameter("@UPIKYCSTATUS", consumer.UPIKYCSTATUS));
                        command.Parameters.Add(CreateSqlParameter("@teslapayoutmode", consumer.teslapayoutmode));
                        command.Parameters.Add(CreateSqlParameter("@Selfie_image", consumer.Selfie_image));

                        await command.ExecuteNonQueryAsync();
                    }
                }
                using (var connection = new SqlConnection(connectionString))
                {
                    string selectQuery1 = $"SELECT M_Consumerid, MobileNo, ConsumerName FROM M_Consumer WHERE User_ID = @UserId";
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(selectQuery1, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", UserId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var response = new
                                {
                                    UserId = UserId,
                                    M_Consumerid = reader["M_Consumerid"],
                                    MobileNo = reader["MobileNo"]
                                };

                                return Ok(new ApiResponse<object>(true, "Consumer created successfully", response));
                            }
                            else
                            {
                                return NotFound(new ApiResponse<object>(false, "No consumer found"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Internal Server Error: {ex.Message}"));
            }
        }

        private SqlParameter CreateSqlParameter(string paramName, object value)
        {
            if (value == null)
                return new SqlParameter(paramName, DBNull.Value);

            // Explicitly check if value is numeric
            if (value is int)
                return new SqlParameter(paramName, SqlDbType.Int) { Value = value };
            else if (value is decimal)
                return new SqlParameter(paramName, SqlDbType.Decimal) { Value = value };
            else if (value is long)
                return new SqlParameter(paramName, SqlDbType.BigInt) { Value = value };
            else if (value is double)
                return new SqlParameter(paramName, SqlDbType.Float) { Value = value };
            else if (value is string)
                return new SqlParameter(paramName, SqlDbType.NVarChar) { Value = value };
            else if (value is DateTime)
                return new SqlParameter(paramName, SqlDbType.DateTime) { Value = value };

            return new SqlParameter(paramName, value);
        }






        [HttpGet("Registration/{M_Consumerid}")]
        public async Task<IActionResult> GetConsumer(int consumerId)
        {
            // SQL connection string
            var connectionString = _configuration.GetConnectionString("defaultConnectionbeta");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // SQL query to fetch consumer by Consumerid
                var query = "SELECT * FROM Consumers WHERE M_Consumerid = @M_Consumerid";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@M_Consumerid", consumerId);

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            var consumer = new Consumer
                            {
                                M_Consumerid = reader.GetInt32(reader.GetOrdinal("M_Consumerid")),
                                User_ID = reader.GetString(reader.GetOrdinal("User_ID")),
                                ConsumerName = reader.GetString(reader.GetOrdinal("ConsumerName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Mobile = reader.GetString(reader.GetOrdinal("MobileNo")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                PinCode = reader.GetString(reader.GetOrdinal("PinCode")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                Entry_Date = reader.GetDateTime(reader.GetOrdinal("Entry_Date")),
                                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")),
                                IsDelete = reader.GetInt32(reader.GetOrdinal("IsDelete")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Per_Address = reader.GetString(reader.GetOrdinal("Per_Address")),
                                ReferralCode = reader.GetInt32(reader.GetOrdinal("ReferralCode")),
                                IsSharedReferralCode = reader.GetBoolean(reader.GetOrdinal("IsSharedReferralCode")),
                                employeeID = reader.GetString(reader.GetOrdinal("employeeID")),
                                distributorID = reader.GetString(reader.GetOrdinal("distributorID")),
                                aadharNumber = reader.GetString(reader.GetOrdinal("aadharNumber")),
                                aadharFile = reader.GetString(reader.GetOrdinal("aadharFile")),
                                aadharback = reader.GetString(reader.GetOrdinal("aadharback")),
                                aadharUploadedate = reader.GetDateTime(reader.GetOrdinal("aadharUploadedate")),
                                aadharUploadedBy = reader.GetString(reader.GetOrdinal("aadharUploadedBy")),
                                Aadhar_source = reader.GetString(reader.GetOrdinal("Aadhar_source")),
                                village = reader.GetString(reader.GetOrdinal("village")),
                                district = reader.GetString(reader.GetOrdinal("district")),
                                state = reader.GetString(reader.GetOrdinal("state")),
                                country = reader.GetString(reader.GetOrdinal("country")),
                                Role_Id = reader.GetInt32(reader.GetOrdinal("Role_Id")),
                                Created_by = reader.GetInt32(reader.GetOrdinal("Created_by")),
                                Comp_id = reader.GetString(reader.GetOrdinal("Comp_id")),
                                SellerName = reader.GetString(reader.GetOrdinal("SellerName")),
                                token = reader.GetString(reader.GetOrdinal("token")),
                                MStarId = reader.GetInt32(reader.GetOrdinal("MStarId")),
                                Inox_User_Type = reader.GetString(reader.GetOrdinal("Inox_User_Type")),
                                Vrkabel_User_Type = reader.GetInt32(reader.GetOrdinal("Vrkabel_User_Type")),
                                cin_number = reader.GetString(reader.GetOrdinal("cin_number")),
                                ref_cin_number = reader.GetString(reader.GetOrdinal("ref_cin_number")),
                                designation = reader.GetString(reader.GetOrdinal("designation")),
                                dob = reader.GetString(reader.GetOrdinal("dob")),
                                gender = reader.GetString(reader.GetOrdinal("gender")),
                                sur_name = reader.GetString(reader.GetOrdinal("sur_name")),
                                communication_status = reader.GetInt32(reader.GetOrdinal("communication_status")),
                                business_status = reader.GetInt32(reader.GetOrdinal("business_status")),
                                house_number = reader.GetString(reader.GetOrdinal("house_number")),
                                land_mark = reader.GetString(reader.GetOrdinal("land_mark")),
                                owner_number = reader.GetString(reader.GetOrdinal("owner_number")),
                                shop_name = reader.GetString(reader.GetOrdinal("shop_name")),
                                pancard_number = reader.GetString(reader.GetOrdinal("pancard_number")),
                                gst_number = reader.GetString(reader.GetOrdinal("gst_number")),
                                pan_card_file = reader.GetString(reader.GetOrdinal("pan_card_file")),
                                shop_file = reader.GetString(reader.GetOrdinal("shop_file")),
                                Other_Role = reader.GetString(reader.GetOrdinal("Other_Role")),
                                profile_image = reader.GetString(reader.GetOrdinal("profile_image")),
                                VRKbl_KYC_status = reader.GetInt32(reader.GetOrdinal("VRKbl_KYC_status")),
                                Additional = reader.GetString(reader.GetOrdinal("Additional")),
                                remark = reader.GetString(reader.GetOrdinal("remark")),
                                panekycStatus = reader.GetString(reader.GetOrdinal("panekycStatus")),
                                aadharkycStatus = reader.GetString(reader.GetOrdinal("aadharkycStatus")),
                                bankekycStatus = reader.GetString(reader.GetOrdinal("bankekycStatus")),
                                PanHolderName = reader.GetString(reader.GetOrdinal("PanHolderName")),
                                AadharHolderName = reader.GetString(reader.GetOrdinal("AadharHolderName")),
                                Shop_address = reader.GetString(reader.GetOrdinal("Shop_address")),
                                FirmName = reader.GetString(reader.GetOrdinal("FirmName")),
                                Apptoken = reader.GetString(reader.GetOrdinal("Apptoken")),
                                AppVersion = reader.GetString(reader.GetOrdinal("AppVersion")),
                                Agegroup = reader.GetString(reader.GetOrdinal("Agegroup")),
                                Pancard_Status = reader.GetString(reader.GetOrdinal("Pancard_Status")),
                                BrandId = reader.GetInt32(reader.GetOrdinal("BrandId")),
                                Aadhar_Status = reader.GetString(reader.GetOrdinal("Aadhar_Status")),
                                Passbook_Status = reader.GetString(reader.GetOrdinal("Passbook_Status")),
                                Ekyc_status = reader.GetString(reader.GetOrdinal("Ekyc_status")),
                                Location = reader.GetString(reader.GetOrdinal("Location")),
                                User_Type = reader.GetInt32(reader.GetOrdinal("User_Type")),
                                AddressProof = reader.GetString(reader.GetOrdinal("AddressProof")),
                                UpiidImage = reader.GetString(reader.GetOrdinal("UpiidImage")),
                                UPIKYCSTATUS = reader.GetInt32(reader.GetOrdinal("UPIKYCSTATUS")),
                                teslapayoutmode = reader.GetString(reader.GetOrdinal("teslapayoutmode")),
                                Selfie_image = reader.GetString(reader.GetOrdinal("Selfie_image"))
                            };

                            return Ok(new ApiResponse<Consumer>(true, "Consumer data fetched successfully", consumer));
                        }
                        else
                        {
                            return NotFound(new ApiResponse<object>(false, "Consumer not found"));
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new ApiResponse<object>(false, $"Error: {ex.Message}"));
                    }
                }
            }
        }
    }
}
