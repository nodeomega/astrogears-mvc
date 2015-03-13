// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AspectInterpretation.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   The Aspect Interpretation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    /// <summary>
    /// The Aspect Interpretation
    /// </summary>
    public partial class AspectInterpretation
    {
        /// <summary>
        /// Aspect Interpretation Types.
        /// </summary>
        public enum InterpretationTypes : byte
        {
            /// <summary>
            /// The natal
            /// </summary>
            Natal = 1,

            /// <summary>
            /// The transit
            /// </summary>
            Transit = 2,

            /// <summary>
            /// The progression
            /// </summary>
            Progression = 3,

            /// <summary>
            /// The synastry
            /// </summary>
            Synastry = 4,

            /// <summary>
            /// The composite
            /// </summary>
            Composite = 5,

            /// <summary>
            /// The davison
            /// </summary>
            Davison = 6,

            /// <summary>
            /// The heliocentric
            /// </summary>
            Heliocentric = 9,

            /// <summary>
            /// The solar return
            /// </summary>
            SolarReturn = 20,

            /// <summary>
            /// The lunar return
            /// </summary>
            LunarReturn = 21,

            /// <summary>
            /// The mercury return
            /// </summary>
            MercuryReturn = 22,

            /// <summary>
            /// The venus return
            /// </summary>
            VenusReturn = 23,

            /// <summary>
            /// The mars return
            /// </summary>
            MarsReturn = 24,

            /// <summary>
            /// The jupiter return
            /// </summary>
            JupiterReturn = 25,

            /// <summary>
            /// The saturn return
            /// </summary>
            SaturnReturn = 26,

            /// <summary>
            /// The solar eclipse
            /// </summary>
            SolarEclipse = 30,

            /// <summary>
            /// The lunar eclipse
            /// </summary>
            LunarEclipse = 31
        }
    }
}