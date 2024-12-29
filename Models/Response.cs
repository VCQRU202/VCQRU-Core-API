namespace CoreApi_BL_App.Models
{


    public class Response
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
        public Object Consumerdtls { get; set; }
        public int Noti_Count { get; set; }
        public string FilePath { get; set; }
        public bool TransactionStaus { get; set; }
    }

    public class Query_responses
    {
        public string totalCode { get; set; }
        public string successCode { get; set; }
        public string totalCash { get; set; }
        public string transferredCash { get; set; }
        public string totalPoint { get; set; }
        public string reedemPoint { get; set; }
        public string totalWarranty { get; set; }
        public string underWarranty { get; set; }
        public string totalcounterfeit { get; set; }
        public string successcounterfeit { get; set; }
        public Query_responses()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }

   
    public class message_status
    {

        public string status { get; set; }
        public string message { get; set; }
        public string fields { get; set; }
    }
    public class location
    {
        public string code1 { get; set; }
        public string code2 { get; set; }
        public string mobileno { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string address { get; set; }
        public DateTime EnqDate { get; set; }
        public string PostalCode { get; set; }
    }

    public class verifyKycDataDetail
    {

        public string M_Consumerid { get; set; }
        public string AadharName { get; set; }
        public string AadharRefrenceId { get; set; }
        public bool IsaadharVerify { get; set; }
        public string PanName { get; set; }
        public string PanRefrenceId { get; set; }
        public bool IspanVerify { get; set; }
        public string AccountHolderName { get; set; }
        public string BankRefrenceId { get; set; }
        public bool IsBankAccountVerify { get; set; }
        public bool Status { get; set; }
        public string PanRemarks { get; set; }
        public string AadharRemarks { get; set; }
        public string BankRemarks { get; set; }
        public bool UploadDocReq { get; set; }
        public string ResponseCode { get; set; }

        public string Msg { get; set; }

    }

}