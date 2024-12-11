namespace CoreApi_BL_App.Models.Vendor
{
    public class UserRegistrationFields
    {
        public FieldData MobileNumber { get; set; }
        public FieldData Name { get; set; }
        public FieldData Email { get; set; }
        public FieldData PinCode { get; set; }
        public FieldData State { get; set; }
        public FieldData City { get; set; }
        public FieldData District { get; set; }
        public FieldData Address { get; set; }
        public FieldData DateOfBirth { get; set; }
        public FieldData Gender { get; set; }
        public FieldData MaritalStatus { get; set; }
        public FieldData UserType { get; set; }

        public FieldData Referral { get; set; }
    }

    public class FieldData
    {

        public string Value { get; set; }

        public bool IsActive { get; set; }

        public bool IsMandatory { get; set; }
    }
}
