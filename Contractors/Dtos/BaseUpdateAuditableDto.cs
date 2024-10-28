namespace Contractors.Dtos
{
    /// <summary>
    /// مدل پایه‌ای برای نگهداری اطلاعات به‌روزرسانی و حذف.
    /// </summary>
    public class BaseUpdateAuditableDto
    {
        /// <summary>
        /// تاریخ آخرین به‌روزرسانی.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین به‌روزرسانی را انجام داده است.
        /// </summary>
        public int UpdatedBy { get; set; }

        /// <summary>
        /// وضعیت حذف شدن داده. اگر true باشد، داده حذف شده است.
        /// </summary>
        public bool? IsDeleted { get; set; }
    }

}
