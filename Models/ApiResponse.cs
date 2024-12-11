namespace CoreApi_BL_App.Models
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(bool status, string message, T data = default)
        {
            Status = status;
            Message = message;
            Data = data;
        }
    }

}
