// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Aspect.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    /// <summary>
    ///     The Aspect.
    /// </summary>
    public partial class Aspect
    {
        #region Enums

        /// <summary>
        ///     Aspect Types
        /// </summary>
        public enum AspectTypes : byte
        {
            /// <summary>
            /// The conjunction.
            /// </summary>
            Conjunction = 0, 

            /// <summary>
            /// The opposition.
            /// </summary>
            Opposition = 1, 

            /// <summary>
            /// The square.
            /// </summary>
            Square = 2, 

            /// <summary>
            /// The semisquare.
            /// </summary>
            Semisquare = 3, 

            /// <summary>
            /// The sesquiquadrate.
            /// </summary>
            Sesquiquadrate = 4, 

            /// <summary>
            /// The trine.
            /// </summary>
            Trine = 5, 

            /// <summary>
            /// The sextile.
            /// </summary>
            Sextile = 6, 

            /// <summary>
            /// The quincunx.
            /// </summary>
            Quincunx = 7, 

            /// <summary>
            /// The quintile.
            /// </summary>
            Quintile = 8, 

            /// <summary>
            /// The biquintile.
            /// </summary>
            Biquintile = 9, 

            /// <summary>
            /// The semisextile.
            /// </summary>
            Semisextile = 10, 

            /// <summary>
            /// The septile.
            /// </summary>
            Septile = 11, 

            /// <summary>
            /// The biseptile.
            /// </summary>
            Biseptile = 12, 

            /// <summary>
            /// The triseptile.
            /// </summary>
            Triseptile = 13, 

            /// <summary>
            /// The novile.
            /// </summary>
            Novile = 14, 

            /// <summary>
            /// The decile.
            /// </summary>
            Decile = 15
        }

        #endregion
    }
}