namespace Contractors.Results
{
    public class Result<T> where T : class
    {
        public T? Data { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public Result<T> WithValue(T? data)
        {
            Data = data;
            return this;
        }
        public Result<T> Success(string message)
        {
            
            Message = message;
            IsSuccessful = true;
            return this;
        }
        public Result<T> Failure(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccessful = false;
            return this;
        }
    }
}
