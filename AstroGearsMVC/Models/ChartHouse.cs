// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartHouse.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the ChartHouse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    using JetBrains.Annotations;

    /// <summary>
    /// The Chart House.
    /// </summary>
    public partial class ChartHouse
    {
        /// <summary>
        ///     Gets the coordinate in seconds.
        /// </summary>
        /// <value>
        ///     The coordinate in seconds.
        /// </value>
        public int CoordinateInSeconds
        {
            get
            {
                return (((this.SignId * 30) + this.Degrees) * 3600) + (this.Minutes * 60) + this.Seconds;
            }
        }

        /// <summary>
        /// Gets the draconic chart house.
        /// </summary>
        /// <param name="northNode">The north node.</param>
        /// <returns>The Draconic version of this chart object based on North Node.</returns>
        public ChartHouse GetDraconicChartHouse([CanBeNull] ChartObject northNode)
        {
            if (northNode == null)
            {
                return null;
            }

            const int SecondsInMinutes = 60;
            const int MinutesInDegrees = 60;
            const int DegreesInSign = 30;
            const int SignsInChart = 12;

            var newCoordinateInSeconds = this.CoordinateInSeconds - northNode.CoordinateInSeconds;

            while (newCoordinateInSeconds < 0)
            {
                newCoordinateInSeconds += SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes;
            }

            while (newCoordinateInSeconds > SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes)
            {
                newCoordinateInSeconds %= SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes;
            }

            var newSignId = (byte)(newCoordinateInSeconds / (DegreesInSign * MinutesInDegrees * SecondsInMinutes));

            var deg = (newCoordinateInSeconds / (MinutesInDegrees * SecondsInMinutes)) % DegreesInSign;
            var min = (newCoordinateInSeconds / SecondsInMinutes) % MinutesInDegrees;
            var sec = newCoordinateInSeconds % SecondsInMinutes;

            return new ChartHouse
                       {
                           EnteredChart = this.EnteredChart,
                           EnteredChartId = this.EnteredChartId,
                           Degrees = (byte)deg,
                           Minutes = (byte)min,
                           Seconds = (byte)sec,
                           SignId = newSignId,
                           Sign = new AstroGearsEntities().Signs.Find(newSignId),
                           HouseCusp =
                               new HouseCusp
                                   {
                                       HouseCuspName = "Dr. " + this.HouseCusp.HouseCuspName,
                                       HouseId = this.HouseCusp.HouseId,
                                       HouseName = this.HouseCusp.HouseName
                                   },
                           ChartHouseId = this.ChartHouseId,
                           HouseId = this.HouseId
                       };
        }

    }
}