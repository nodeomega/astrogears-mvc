// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnteredChart.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    using JetBrains.Annotations;

    /// <summary>
    ///     The EnteredChart
    /// </summary>
    public partial class EnteredChart
    {
        #region Enums

        /// <summary>
        ///     Chart Types
        /// </summary>
        public enum ChartTypes : byte
        {
            /// <summary>
            ///     Natal Charts.
            /// </summary>
            Natal = 1, 

            /// <summary>
            ///     Event Charts (Job beginnings, separations, etc.)
            /// </summary>
            Event = 2, 

            /// <summary>
            ///     Transit Charts.
            /// </summary>
            Transit = 3, 

            /// <summary>
            ///     Progressed Charts.
            /// </summary>
            Progressed = 4, 

            /// <summary>
            /// Solar Arcs
            /// </summary>
            SolarArc = 5,

            /// <summary>
            ///     Heliocentric charts.
            /// </summary>
            Heliocentric = 9, 

            /// <summary>
            ///     Composite charts (Midpoint method)
            /// </summary>
            CompositeMidpoint = 10, 

            /// <summary>
            ///     Composite charts (Reference Place method)
            /// </summary>
            CompositeReferencePlace = 11, 

            /// <summary>
            ///     Davison Chart (Corrected)
            /// </summary>
            DavisonCorrected = 12, 

            /// <summary>
            ///     Davison Chart (Uncorrected)
            /// </summary>
            DavisonUncorrected = 13, 

            /// <summary>
            ///     Solar Return Charts
            /// </summary>
            SolarReturn = 20, 

            /// <summary>
            ///     Lunar Return Charts
            /// </summary>
            LunarReturn = 21, 

            /// <summary>
            ///     Mercury Return Charts
            /// </summary>
            MercuryReturn = 22, 

            /// <summary>
            ///     Venus Return Charts
            /// </summary>
            VenusReturn = 23, 

            /// <summary>
            ///     Mars Return Charts
            /// </summary>
            MarsReturn = 24, 

            /// <summary>
            ///     Jupiter Return Charts
            /// </summary>
            JupiterReturn = 25, 

            /// <summary>
            ///     Saturn Return Charts
            /// </summary>
            SaturnReturn = 26, 

            /// <summary>
            ///     The solar eclipse
            /// </summary>
            SolarEclipse = 30, 

            /// <summary>
            ///     The lunar eclipse
            /// </summary>
            LunarEclipse = 31
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the origin date time string.
        /// </summary>
        /// <value>
        ///     The origin date time string.
        /// </value>
        [NotNull]
        public string OriginDateTimeString
        {
            get
            {
                if (this.OriginDateTimeUnknown)
                {
                    return this.OriginDateTime.ToString("MM/dd/yyyy") + " ??:??";
                }

                return this.OriginDateTime.ToString("MM/dd/yyyy HH:mm");
            }
        }

        #endregion
    }
}