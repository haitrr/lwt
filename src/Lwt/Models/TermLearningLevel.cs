namespace Lwt.Models
{
    /// <summary>
    /// the term's learning levels.
    /// </summary>
    public enum TermLearningLevel
    {
        /// <summary>
        /// ignored terms.
        /// </summary>
        Ignored = 0,

        /// <summary>
        /// not know yet.
        /// </summary>
        UnKnow = 1,

        /// <summary>
        /// learning level 1.
        /// </summary>
        Learning1 = 2,

        /// <summary>
        /// learning level 2.
        /// </summary>
        Learning2 = 3,

        /// <summary>
        /// learning level 3.
        /// </summary>
        Learning3 = 4,

        /// <summary>
        /// learning level 4.
        /// </summary>
        Learning4 = 5,

        /// <summary>
        /// learning level 5.
        /// </summary>
        Learning5 = 6,

        /// <summary>
        /// well know.
        /// </summary>
        WellKnow = 7,
    }
}