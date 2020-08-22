namespace Lwt.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// language code.
    /// </summary>
    [JsonConverter(typeof(LearningLevelJsonConverter))]
    public sealed class LearningLevel
    {
        /// <summary>
        /// Skipped.
        /// </summary>
        public static readonly LearningLevel Skipped = new LearningLevel(LearningLevelValue.Skipped);

        /// <summary>
        /// Ignored.
        /// </summary>
        public static readonly LearningLevel Ignored = new LearningLevel(LearningLevelValue.Ignored);

        /// <summary>
        /// Unknown.
        /// </summary>
        public static readonly LearningLevel Unknown = new LearningLevel(LearningLevelValue.Unknow);

        /// <summary>
        /// Learning 1.
        /// </summary>
        public static readonly LearningLevel Learning1 = new LearningLevel(LearningLevelValue.Learning1);

        /// <summary>
        /// Learning 2.
        /// </summary>
        public static readonly LearningLevel Learning2 = new LearningLevel(LearningLevelValue.Learning2);

        /// <summary>
        /// Learning 3.
        /// </summary>
        public static readonly LearningLevel Learning3 = new LearningLevel(LearningLevelValue.Learning3);

        /// <summary>
        /// Learning 4.
        /// </summary>
        public static readonly LearningLevel Learning4 = new LearningLevel(LearningLevelValue.Learning4);

        /// <summary>
        /// Learning 5.
        /// </summary>
        public static readonly LearningLevel Learning5 = new LearningLevel(LearningLevelValue.Learning5);

        /// <summary>
        /// Learning1.
        /// </summary>
        public static readonly LearningLevel WellKnown = new LearningLevel(LearningLevelValue.WellKnown);

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningLevel"/> class.
        /// </summary>
        /// <param name="value"> value.</param>
        private LearningLevel(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets value of code.
        /// </summary>
        public string Value { get; protected set; }

        /// <summary>
        /// get language code from string.
        /// </summary>
        /// <param name="code">code.</param>
        /// <returns>language code.</returns>
        /// <exception cref="NotSupportedException">language code is not supported.</exception>
        public static LearningLevel GetFromString(string code)
        {
            switch (code)
            {
                case LearningLevelValue.Skipped:
                    return Skipped;
                case LearningLevelValue.Unknow:
                    return Unknown;
                case LearningLevelValue.Ignored:
                    return Ignored;
                case LearningLevelValue.Learning1:
                    return Learning1;
                case LearningLevelValue.Learning2:
                    return Learning2;
                case LearningLevelValue.Learning3:
                    return Learning3;
                case LearningLevelValue.Learning4:
                    return Learning4;
                case LearningLevelValue.Learning5:
                    return Learning5;
                case LearningLevelValue.WellKnown:
                    return WellKnown;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        ///  get all learning levels.
        /// </summary>
        /// <returns>all learning levels.</returns>
        public static IEnumerable<LearningLevel> GetAll()
        {
            return new[]
            {
                Skipped, Ignored, Unknown, Learning1, Learning2, Learning3, Learning4, Learning5, WellKnown,
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Value;
        }
    }
}