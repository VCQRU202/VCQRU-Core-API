namespace CoreApi_BL_App.Models
{
    public class bank_responses
    {
        public string Bank_ID { get; set; }
        public string Bank_Name { get; set; }
        public string Account_HolderNm { get; set; }
        public string Account_No { get; set; }
        public string Branch { get; set; }
        public string IFSC_Code { get; set; }
        public string City { get; set; }
        public string RTGS_Code { get; set; }
        public string Address { get; set; }
        public string Account_Type { get; set; }
        public string chkpassbook { get; set; }
        public int M_Consumerid { get; set; }
        public string DML { get; set; }
        public DateTime Entry_Date { get; set; }
        public bool Flag { get; set; }
        public string Comp_id { get; set; }
        public bank_responses()
        {
            //
            // TODO: Add constructor logic here
            //
        }

    }
}
