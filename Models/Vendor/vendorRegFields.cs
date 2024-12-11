namespace CoreApi_BL_App.Models.Vendor
{


    public class FieldSetting
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string hint { get; set; }

        public List<string> Values { get; set; }

        public string regex { get; set; } 
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }


    }

    public class FormFieldsRequest
    {
        public List<FieldSetting> Fields { get; set; }
    }


}
