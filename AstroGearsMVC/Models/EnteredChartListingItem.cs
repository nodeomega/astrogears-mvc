// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnteredChartListingItem.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the EnteredChartListingItem type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    /// <summary>
    /// Class EnteredChartListingItem.
    /// </summary>
    public class EnteredChartListingItem
    {
        /// <summary>
        /// Gets or sets the name of the chart type.
        /// </summary>
        /// <value>The name of the chart type.</value>
        public string ChartTypeName { get; set; }

        /// <summary>
        /// Gets or sets the entered chart identifier.
        /// </summary>
        /// <value>The entered chart identifier.</value>
        public int EnteredChartId { get; set; }

        /// <summary>
        /// Gets or sets the origin date time string.
        /// </summary>
        /// <value>The origin date time string.</value>
        public string OriginDateTimeString { get; set; }

        /// <summary>
        /// Gets or sets the subject location.
        /// </summary>
        /// <value>The subject location.</value>
        public string SubjectLocation { get; set; }

        /// <summary>
        /// Gets or sets the name of the subject.
        /// </summary>
        /// <value>The name of the subject.</value>
        public string SubjectName { get; set; }
    }
}