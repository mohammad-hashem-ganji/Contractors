namespace Contractors.Results
{
    public class Result<T> where T : class
    {
        /// <summary>
        /// داده‌های برگشتی در نتیجه.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// نشان می‌دهد که عملیات موفقیت‌آمیز بوده است یا خیر.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// پیام موفقیت در صورت موفقیت‌آمیز بودن عملیات.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// پیام خطا در صورت شکست عملیات.
        /// </summary>
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
