namespace Lwt.Models
{
    /// <summary>
    /// the pagination query.
    /// </summary>
    public class PaginationQuery
    {
        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the amount of item per page.
        /// </summary>
        public int ItemPerPage { get; set; }
    }
}