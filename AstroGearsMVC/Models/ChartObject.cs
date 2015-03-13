// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartObject.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Models
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    ///     The Chart Object.
    /// </summary>
    public partial class ChartObject
    {
        #region Enums

        /// <summary>
        ///     Object Types.
        /// </summary>
        public enum ObjectTypes : byte
        {
            /// <summary>
            ///     Represents a major planet (Mercury through Neptune plus Pluto) or luminary (Sun/Moon).
            /// </summary>
            MajorPlanetLuminary = 1, 

            /// <summary>
            ///     Represents an asteroid.
            /// </summary>
            Asteroid = 2, 

            /// <summary>
            ///     Represents an Arabic Part.
            /// </summary>
            ArabicPart = 3, 

            /// <summary>
            ///     Represents a fixed star.
            /// </summary>
            FixedStar = 4, 

            /// <summary>
            ///     Represents the angles and house cusps.
            /// </summary>
            AngleHouseCusp = 5, 

            /// <summary>
            ///     Represents the nodes.
            /// </summary>
            Nodes = 6
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the calculated coordinate.
        /// </summary>
        /// <value>
        ///     The calculated coordinate.
        /// </value>
        public decimal CalculatedCoordinate
        {
            get
            {
                return ((decimal)this.SignId * 30) + this.Degrees + ((decimal)this.Minutes / 60)
                       + ((decimal)this.Seconds / 3600);
            }
        }

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
        /// Gets or sets the angle identifier.
        /// </summary>
        /// <value>
        /// The angle identifier.
        /// </value>
        public byte? AngleId { get; set; }

        /// <summary>
        /// Gets or sets the arabic part identifier.
        /// </summary>
        /// <value>
        /// The arabic part identifier.
        /// </value>
        public int? ArabicPartId { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the draconic chart object.
        /// </summary>
        /// <param name="northNode">
        /// The north node.
        /// </param>
        /// <returns>
        /// The Draconic version of this chart object based on North Node.
        /// </returns>
        [CanBeNull]
        public ChartObject GetDraconicChartObject([CanBeNull] ChartObject northNode)
        {
            if (northNode == null)
            {
                return null;
            }

            if (this.CelestialObject.Draconic)
            {
                // if already draconic, return itself.
                return this;
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

            return new ChartObject
                       {
                           EnteredChart = this.EnteredChart, 
                           EnteredChartID = this.EnteredChartID, 
                           Degrees = (byte)deg, 
                           Minutes = (byte)min, 
                           Seconds = (byte)sec, 
                           SignId = newSignId, 
                           Sign = new AstroGearsEntities().Signs.Find(newSignId), 
                           CelestialObject =
                               new CelestialObject
                                   {
                                       AllowableOrb = this.CelestialObject.AllowableOrb, 
                                       AlternateName =
                                           "Dr. " + this.CelestialObject.AlternateName, 
                                       CelestialObjectId = 0, 
                                       CelestialObjectName =
                                           "Dr. " + this.CelestialObject.CelestialObjectName, 
                                       Draconic = true, 
                                       CelestialObjectType =
                                           new CelestialObjectType
                                               {
                                                   CelestialObjectTypeName =
                                                       this.CelestialObject
                                                       .CelestialObjectType
                                                       .CelestialObjectTypeName, 
                                                   CelestialObjectTypeId =
                                                       this.CelestialObject
                                                       .CelestialObjectType
                                                       .CelestialObjectTypeId
                                               }, 
                                       CelestialObjectTypeId =
                                           this.CelestialObject.CelestialObjectTypeId
                                   }, 
                           CelestialObjectId = this.CelestialObjectId, 
                           Orientation =
                               new Orientation
                                   {
                                       OrientationId = this.Orientation.OrientationId, 
                                       OrientationAbbreviation =
                                           this.Orientation.OrientationAbbreviation
                                   }, 
                           OrientationId = this.OrientationId
                       };
        }

        /// <summary>
        /// Determines whether the specified compare is biquintile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if biquintile; false otherwise.
        /// </returns>
        public bool IsBiquintile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 2M ? 2M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 144;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is biseptile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if biseptile; false otherwise.
        /// </returns>
        public bool IsBiseptile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1.5M ? 1.5M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = (360M / 7M) * 2M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is conjunct.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if conjunct; false otherwise.
        /// </returns>
        public bool IsConjunct([NotNull] ChartObject compare)
        {
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 0M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is decile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if decile; false otherwise.
        /// </returns>
        public bool IsDecile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1M ? 1M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 36M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is novile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if novile; false otherwise.
        /// </returns>
        public bool IsNovile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1M ? 1M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 40M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is opposite.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if opposite; false otherwise.
        /// </returns>
        public bool IsOpposite([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 180M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is quincunx.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if quincunx; false otherwise.
        /// </returns>
        public bool IsQuincunx([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 2M ? 2M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 150M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is quintile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if quintile; false otherwise.
        /// </returns>
        public bool IsQuintile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 2M ? 2M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 72M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether [is semi sextile] [the specified compare].
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if semisextile; false otherwise.
        /// </returns>
        public bool IsSemiSextile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1.5M ? 1.5M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 30M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is semisquare.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if semisquare; false otherwise.
        /// </returns>
        public bool IsSemisquare([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 2M ? 2M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 45M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is septile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if septile; false otherwise.
        /// </returns>
        public bool IsSeptile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1.5M ? 1.5M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 360M / 7M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is sesquiquadrate.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if sesquiquadrate; false otherwise.
        /// </returns>
        public bool IsSesquiquadrate([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 2M ? 2M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 135M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is sextile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if sextile; false otherwise.
        /// </returns>
        public bool IsSextile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 60M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is square.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if square; false otherwise.
        /// </returns>
        public bool IsSquare([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 90M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is trine.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if trine; false otherwise.
        /// </returns>
        public bool IsTrine([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = 120M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        /// <summary>
        /// Determines whether the specified compare is triseptile.
        /// </summary>
        /// <param name="compare">
        /// The compare.
        /// </param>
        /// <returns>
        /// True if triseptile; false otherwise.
        /// </returns>
        public bool IsTriseptile([NotNull] ChartObject compare)
        {
            // If fixed star is either of the objects, automatic false (Only conjunctions and possibly oppositions count).
            if (compare == null)
            {
                throw new ArgumentNullException("compare");
            }

            if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar)
                || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.FixedStar))
            {
                return false;
            }

            decimal thisOrb;

            if (compare.CelestialObject.CelestialObjectTypeId == this.CelestialObject.CelestialObjectTypeId)
            {
                thisOrb = (compare.CelestialObject.AllowableOrb + this.CelestialObject.AllowableOrb) / 2M;
            }
            else if ((compare.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid)
                     || (this.CelestialObject.CelestialObjectTypeId == (byte)ObjectTypes.Asteroid))
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }
            else
            {
                thisOrb = Math.Min(compare.CelestialObject.AllowableOrb, this.CelestialObject.AllowableOrb);
            }

            thisOrb = thisOrb > 1.5M ? 1.5M : thisOrb;

            var difference = Math.Abs(compare.CalculatedCoordinate - this.CalculatedCoordinate);
            const decimal DegreeCheck = (360M / 7M) * 3M;
            return ((difference >= DegreeCheck - thisOrb) && (difference <= DegreeCheck + thisOrb))
                   || ((difference >= (360 - DegreeCheck) - thisOrb) && (difference <= (360 - DegreeCheck) + thisOrb));
        }

        #endregion
    }
}