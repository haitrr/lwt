namespace Lwt.Models
{
    /// <summary>
    /// the term's learning levels.
    /// </summary>
    public enum TermLearningLevel
    {
        /// <summary>
        /// not know yet.
        /// </summary>
        Unknow = 1,

        /// <summary>
        /// learning level 1.
        /// </summary>
        Learning1 = 3,

        /// <summary>
        /// learning level 2.
        /// </summary>
        Learning2 = 4,

        /// <summary>
        /// learning level 3.
        /// </summary>
        Learning3 = 5,

        /// <summary>
        /// knew.
        /// </summary>
        Knew = 6,

        /// <summary>
        /// well know.
        /// </summary>
        WellKnow = 7,
    }
}