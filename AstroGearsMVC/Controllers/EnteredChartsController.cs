// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnteredChartsController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using AstroGearsMVC.Filters;
    using AstroGearsMVC.Models;

    using JetBrains.Annotations;

    using WebGrease.Css.Extensions;

    /// <summary>
    ///     Entered Charts Controller.
    /// </summary>
    [JsonpFilter]
    public class EnteredChartsController : Controller
    {
        #region Fields

        /// <summary>
        ///     The database
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        #endregion

        // TODO: Create GetRelocatedHouseListing
        #region Public Methods and Operators

        /// <summary>
        /// Creates the angle for entered chart.
        /// </summary>
        /// <param name="enteredChartId">The entered chart identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="angleId">The angle identifier.</param>
        /// <returns>
        /// A JSON result.
        /// </returns>
        [CanBeNull]
        public JsonResult CreateAngleForEnteredChart(
            int enteredChartId, 
            byte degrees, 
            byte signId, 
            byte minutes, 
            byte seconds, 
            byte angleId)
        {
            var newAngle = new ChartAngle
                               {
                                   EnteredChartId = enteredChartId, 
                                   Degrees = degrees, 
                                   SignId = signId, 
                                   Minutes = minutes, 
                                   Seconds = seconds, 
                                   AngleId = angleId
                               };

            this.db.ChartAngles.Add(newAngle);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Creates the aspect chart enumerable.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        /// <param name="aspectChartObjects">The input.</param>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// An IEnumerable(of Object)
        /// </returns>
        [CanBeNull]
        public IEnumerable<object> CreateAspectChartEnumerable(
            [CanBeNull] ChartObject baseObject, 
            [CanBeNull] List<ChartObject> aspectChartObjects, 
            int chartId, 
            byte houseSystemId)
        {
            // Get the houses if available.
            var chartHouses =
                this.db.ChartHouses.Where(
                    houses =>
                    houses.EnteredChartId == chartId && houses.HouseSystemId == houseSystemId && houses.HouseId != 0)
                    .ToList()
                    .OrderBy(houses => houses.CoordinateInSeconds)
                    .Select(x => new { x.Degrees, x.Minutes, x.Seconds, x.Sign.SignId, x.HouseId })
                    .ToList();

            // ReSharper disable once AssignNullToNotNullAttribute
            return
                aspectChartObjects.Select(
                    x =>
                    new
                        {
                            x.CelestialObjectId,
                            x.CelestialObject.CelestialObjectName, 
                            x.Sign.SignAbbreviation, 
                            x.Sign.Element.HtmlTextCssClass, 
                            x.Degrees, 
                            x.Minutes, 
                            x.Seconds, 
                            x.Orientation.OrientationAbbreviation, 
                            x.CelestialObject.CelestialObjectType.CelestialObjectTypeName, 
                            x.CelestialObject.Draconic, 
                            House =
                        (chartHouses.Count > 0)
                            ? (chartHouses.LastOrDefault(
                                h =>
                                (((((h.SignId * 30) + h.Degrees) * 3600) + (h.Minutes * 60) + h.Seconds)
                                 <= ((((x.SignId * 30) + x.Degrees) * 3600) + (x.Minutes * 60) + x.Seconds)))
                               ?? chartHouses.Last()).HouseId
                            : 0,
                            x.AngleId,
                            BaseObjectValidForInterpretation = baseObject != null && (baseObject.CelestialObjectId > 0 || baseObject.AngleId != null),
                            BaseObjectCelestialObjectId = (baseObject != null) ? baseObject.CelestialObjectId : 0,
                            BaseObjectAngleId = (baseObject != null) ? baseObject.AngleId : null,
                            ThisObjectValidForInterpretation = x.CelestialObjectId > 0  || x.AngleId != null
                        });
        }

        /// <summary>
        /// Creates the chart object for entered chart.
        /// </summary>
        /// <param name="enteredChartId">The entered chart identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="orientationId">The orientation identifier.</param>
        /// <param name="celestialObjectId">The celestial object identifier.</param>
        /// <returns>
        /// A JSON result.
        /// </returns>
        public JsonResult CreateChartObjectForEnteredChart(
            int enteredChartId,
            byte degrees,
            byte signId,
            byte minutes,
            byte seconds,
            byte orientationId,
            int celestialObjectId)
        {
            var newObject = new ChartObject
                                {
                                    EnteredChartID = enteredChartId,
                                    Degrees = degrees,
                                    SignId = signId,
                                    Minutes = minutes,
                                    Seconds = seconds,
                                    OrientationId = orientationId,
                                    CelestialObjectId = celestialObjectId
                                };
            this.db.ChartObjects.Add(newObject);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates the house cusp for entered chart.
        /// </summary>
        /// <param name="enteredChartId">The entered chart identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <param name="houseId">The house identifier.</param>
        /// <returns>
        /// A JSON result.
        /// </returns>
        [CanBeNull]
        public JsonResult CreateHouseCuspForEnteredChart(
            int enteredChartId, 
            byte degrees, 
            byte signId, 
            byte minutes, 
            byte seconds, 
            byte houseSystemId, 
            byte houseId)
        {
            var newHouse = new ChartHouse
                               {
                                   EnteredChartId = enteredChartId,
                                   Degrees = degrees,
                                   SignId = signId,
                                   Minutes = minutes,
                                   Seconds = seconds,
                                   HouseSystemId = houseSystemId,
                                   HouseId = houseId
                               };

            this.db.ChartHouses.Add(newHouse);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the entered chart for delete.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The entered chart data to be deleted.</returns>
        [CanBeNull]
        public JsonResult GetEnteredChartForDelete(int? id)
        {
            if (id == null)
            {
                return this.Json("Not Found for Delete", JsonRequestBehavior.AllowGet);
            }

            var thisDelete =
                this.db.EnteredCharts.Where(x => x.EnteredChartId == id.Value)
                    .ToList()
                    .Select(
                        x =>
                        new
                            {
                                x.EnteredChartId,
                                x.SubjectName,
                                x.SubjectLocation,
                                x.OriginDateTimeString,
                                x.ChartType.ChartTypeName,
                                NumberOfChartObjects = x.ChartObjects.Count()
                            });

            return this.Json(thisDelete, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Confirms the delete of entered chart.
        /// </summary>
        /// <param name="enteredChartId">The entered chart identifier.</param>
        /// <returns>A return message</returns>
        [CanBeNull]
        public JsonResult ConfirmDeleteOfEnteredChart(int enteredChartId)
        {
            try
            {
                var deleteEnteredChart = this.db.EnteredCharts.Find(enteredChartId);

                if (deleteEnteredChart == null)
                {
                    return this.Json("Failed: Cannot find specified Entered Chart", JsonRequestBehavior.DenyGet);
                }

                deleteEnteredChart.ChartAngles.ToList().ForEach(x => this.db.ChartAngles.Remove(x));
                deleteEnteredChart.ChartHouses.ToList().ForEach(x => this.db.ChartHouses.Remove(x));
                deleteEnteredChart.ChartObjects.ToList().ForEach(x => this.db.ChartObjects.Remove(x));
                var relos = deleteEnteredChart.RelocatedCharts.ToList();
                foreach (var r in relos)
                {
                    r.RelocatedChartAngles.ToList().ForEach(x => this.db.RelocatedChartAngles.Remove(x));
                    r.RelocatedChartHouses.ToList().ForEach(x => this.db.RelocatedChartHouses.Remove(x));
                }
                deleteEnteredChart.RelocatedCharts.ToList().ForEach(x => this.db.RelocatedCharts.Remove(x));

                this.db.EnteredCharts.Remove(deleteEnteredChart);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Gets the entered chart for edit.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The entered chart data to be updated.</returns>
        [CanBeNull]
        public JsonResult GetEnteredChartForEdit(int? id)
        {
            if (id == null)
            {
                return this.Json("Not Found for Update", JsonRequestBehavior.AllowGet);
            }

            var thisUpdate =
                this.db.EnteredCharts.Where(x => x.EnteredChartId == id.Value).ToList()
                    .Select(
                        x =>
                        new
                        {
                            x.EnteredChartId,
                            x.SubjectName,
                            x.SubjectLocation,
                            OriginDateTime = x.OriginDateTime.ToString("MM/dd/yyyy HH:mm"),
                            x.OriginDateTimeUnknown,
                            x.ChartTypeId
                        });

            return this.Json(thisUpdate, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the chart types list.
        /// </summary>
        /// <returns>A JSON array of the Chart Types.</returns>
        [CanBeNull]
        public JsonResult GetChartTypesList()
        {
            var aspectTypes = this.db.ChartTypes.Select(x => new { x.ChartTypeId, x.ChartTypeName });

            return this.Json(aspectTypes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the entered chart details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The Details.
        /// </returns>
        [NotNull]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ////var enteredChart = await this.db.EnteredCharts.Include(chart => chart.ChartObjects).FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            var enteredChart = await this.db.EnteredCharts.FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            if (enteredChart == null)
            {
                return this.HttpNotFound();
            }

            return this.View(enteredChart);
        }

        /// <summary>
        /// Updates the entered chart.
        /// </summary>
        /// <param name="enteredChartId">The entered chart identifier.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="subjectLocation">The subject location.</param>
        /// <param name="originDateTime">The origin date time.</param>
        /// <param name="originDateTimeUnknown">if set to <c>true</c> [origin date time unknown].</param>
        /// <param name="chartTypeId">The chart type identifier.</param>
        /// <returns>A pass or fail message.</returns>
        [CanBeNull]
        public JsonResult UpdateEnteredChart(
            int enteredChartId,
            [NotNull] string subjectName,
            [NotNull] string subjectLocation,
            DateTime originDateTime,
            bool originDateTimeUnknown,
            byte chartTypeId)
        {
            if (subjectName == null)
            {
                throw new ArgumentNullException("subjectName");
            }

            if (subjectLocation == null)
            {
                throw new ArgumentNullException("subjectLocation");
            }

            try
            {
                var enteredChart = this.db.EnteredCharts.Find(enteredChartId);

                if (enteredChart == null)
                {
                    return this.Json("Failed: Cannot find specified Entered Chart", JsonRequestBehavior.DenyGet);
                }

                enteredChart.SubjectName = subjectName;
                enteredChart.SubjectLocation = subjectLocation;
                enteredChart.OriginDateTime = originDateTime;
                enteredChart.OriginDateTimeUnknown = originDateTimeUnknown;
                enteredChart.ChartTypeId = chartTypeId;

                this.UpdateModel(enteredChart);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Creates the new entered chart.
        /// </summary>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="subjectLocation">The subject location.</param>
        /// <param name="originDateTime">The origin date time.</param>
        /// <param name="originDateTimeUnknown">if set to <c>true</c> [origin date time unknown].</param>
        /// <param name="chartTypeId">The chart type identifier.</param>
        /// <returns>A return message.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// subjectName
        /// or
        /// subjectLocation
        /// </exception>
        [CanBeNull]
        public JsonResult CreateNewEnteredChart(
            [NotNull] string subjectName,
            [NotNull] string subjectLocation,
            DateTime originDateTime,
            bool originDateTimeUnknown,
            byte chartTypeId)
        {
            if (subjectName == null)
            {
                throw new ArgumentNullException("subjectName");
            }

            if (subjectLocation == null)
            {
                throw new ArgumentNullException("subjectLocation");
            }

            try
            {
                var enteredChart = new EnteredChart
                                       {
                                           SubjectName = subjectName,
                                           SubjectLocation = subjectLocation,
                                           OriginDateTime = originDateTime,
                                           OriginDateTimeUnknown = originDateTimeUnknown,
                                           ChartTypeId = chartTypeId,
                                       };
                
                this.db.EnteredCharts.Add(enteredChart);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Gets the angle chart objects.
        /// </summary>
        /// <param name="chartId">
        /// The chart identifier.
        /// </param>
        /// <returns>
        /// The listing of Angles to include in the listing.
        /// </returns>
        [NotNull]
        public List<ChartObject> GetAngleChartObjects(int chartId)
        {
            try
            {
                const byte AngleHouseCuspTypeId = (byte)ChartObject.ObjectTypes.AngleHouseCusp;
                var angleHouseCuspType = this.db.CelestialObjectTypes.Find(AngleHouseCuspTypeId);
                const byte AngleOrientationId = (byte)1;
                var angleOrientation = this.db.Orientations.Find(AngleOrientationId);
                var signs = this.db.Signs.ToList();

                var chartAngles = new List<ChartObject>();

                var baseAngles = this.db.ChartAngles.Where(angles => angles.EnteredChartId == chartId);

                foreach (var x in baseAngles)
                {
                    chartAngles.Add(
                        new ChartObject
                            {
                                EnteredChartID = chartId, 
                                CelestialObject =
                                    new CelestialObject
                                        {
                                            CelestialObjectName = x.HouseAngle.AngleName, 
                                            AlternateName = x.HouseAngle.AngleName.ToUpper(), 
                                            AllowableOrb =
                                                x.HouseAngle.AngleName == "Vertex" ? 1M : 3M, 
                                            CelestialObjectTypeId = AngleHouseCuspTypeId, 
                                            CelestialObjectType = angleHouseCuspType, 
                                            Draconic = false
                                        }, 
                                OrientationId = AngleOrientationId, 
                                Orientation = angleOrientation, 
                                SignId = x.Sign.SignId, 
                                Sign = signs.Find(s => s.SignId == x.Sign.SignId), 
                                Degrees = x.Degrees, 
                                Minutes = x.Minutes, 
                                Seconds = x.Seconds,
                                AngleId = x.AngleId
                            });
                }

                var vertex = chartAngles.FirstOrDefault(x => x.CelestialObject.CelestialObjectName == "Vertex");

                if (vertex != null)
                {
                    var antivertex = new ChartObject
                                         {
                                             EnteredChartID = chartId, 
                                             CelestialObject =
                                                 new CelestialObject
                                                     {
                                                         CelestialObjectName = "Antivertex", 
                                                         AlternateName = "ANTIVERTEX", 
                                                         AllowableOrb = 1M, 
                                                         CelestialObjectTypeId =
                                                             AngleHouseCuspTypeId, 
                                                         CelestialObjectType =
                                                             angleHouseCuspType, 
                                                         Draconic = false
                                                     }, 
                                             OrientationId = AngleOrientationId, 
                                             Orientation = angleOrientation, 
                                             SignId =
                                                 vertex.SignId < 6
                                                     ? (byte)(vertex.SignId + 6)
                                                     : (byte)(vertex.SignId - 6), 
                                             Sign =
                                                 signs.Find(
                                                     s =>
                                                     s.SignId
                                                     == (vertex.SignId < 6
                                                             ? (byte)(vertex.SignId + 6)
                                                             : (byte)(vertex.SignId - 6))), 
                                             Degrees = vertex.Degrees, 
                                             Minutes = vertex.Minutes, 
                                             Seconds = vertex.Seconds,
                                             AngleId = 3
                                         };

                    chartAngles.Add(antivertex);
                }

                var ascendant = chartAngles.FirstOrDefault(x => x.CelestialObject.CelestialObjectName == "Ascendant");

                if (ascendant != null)
                {
                    var descendant = new ChartObject
                                         {
                                             EnteredChartID = chartId, 
                                             CelestialObject =
                                                 new CelestialObject
                                                     {
                                                         CelestialObjectName = "Descendant", 
                                                         AlternateName = "DESCENDANT", 
                                                         AllowableOrb = 3M, 
                                                         CelestialObjectTypeId =
                                                             AngleHouseCuspTypeId, 
                                                         CelestialObjectType =
                                                             angleHouseCuspType, 
                                                         Draconic = false
                                                     }, 
                                             OrientationId = AngleOrientationId, 
                                             Orientation = angleOrientation, 
                                             SignId =
                                                 ascendant.SignId < 6
                                                     ? (byte)(ascendant.SignId + 6)
                                                     : (byte)(ascendant.SignId - 6), 
                                             Sign =
                                                 this.db.Signs.Find(
                                                     ascendant.SignId < 6
                                                         ? (byte)(ascendant.SignId + 6)
                                                         : (byte)(ascendant.SignId - 6)), 
                                             Degrees = ascendant.Degrees, 
                                             Minutes = ascendant.Minutes, 
                                             Seconds = ascendant.Seconds,
                                             AngleId = 4
                                         };

                    chartAngles.Add(descendant);
                }

                var midheaven = chartAngles.FirstOrDefault(x => x.CelestialObject.CelestialObjectName == "Midheaven");

                if (midheaven != null)
                {
                    var imumCoeli = new ChartObject
                                        {
                                            EnteredChartID = chartId, 
                                            CelestialObject =
                                                new CelestialObject
                                                    {
                                                        CelestialObjectName = "Imum Coeli", 
                                                        AlternateName = "IMUM COELI", 
                                                        AllowableOrb = 3M, 
                                                        CelestialObjectTypeId =
                                                            AngleHouseCuspTypeId, 
                                                        CelestialObjectType = angleHouseCuspType, 
                                                        Draconic = false
                                                    }, 
                                            OrientationId = AngleOrientationId, 
                                            Orientation = angleOrientation, 
                                            SignId =
                                                midheaven.SignId < 6
                                                    ? (byte)(midheaven.SignId + 6)
                                                    : (byte)(midheaven.SignId - 6), 
                                            Sign =
                                                this.db.Signs.Find(
                                                    midheaven.SignId < 6
                                                        ? (byte)(midheaven.SignId + 6)
                                                        : (byte)(midheaven.SignId - 6)), 
                                            Degrees = midheaven.Degrees, 
                                            Minutes = midheaven.Minutes, 
                                            Seconds = midheaven.Seconds,
                                            AngleId = 5
                                        };

                    chartAngles.Add(imumCoeli);
                }

                return chartAngles;
            }
            catch
            {
                return new List<ChartObject>();
            }
        }

        /// <summary>
        /// Generates the Arabic Parts/Lots for this chart.
        /// </summary>
        /// <param name="chartId">The identifier.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <returns>
        /// A list of Arabic Parts.
        /// </returns>
        [NotNull]
        public List<ChartObject> GetArabicPartChartObjects(int chartId, byte houseSystemId, bool arabic)
        {
            try
            {
                // Exit without changes if invalid selection selected.
                if (chartId <= 0 || !arabic)
                {
                    return new List<ChartObject>();
                }

                ////var arabicImport = new EnteredChart(id);
                ////var arabicImport = this.db.EnteredCharts.Find(id);
                ////arabicImport.GetObjects();

                // Most arabic parts are dependent on knowing the angles.  Without these, we cannot continue.
                // So angles AND all major planets are required.
                ////var angleCusps = new List<EnteredChartObject>();
                var cuspHouses =
                    this.db.ChartHouses.Include(ch => ch.Sign)
                        .Where(
                            houses =>
                            houses.EnteredChartId == chartId && houses.HouseSystemId == houseSystemId
                            && houses.HouseCusp.HouseCuspName != "Vertex")
                        .OrderBy(a => a.HouseId)
                        .ToList();

                if (cuspHouses.Count != 12)
                {
                    return new List<ChartObject>();
                }

                var chartAngles =
                    this.db.ChartAngles.Include(an => an.Sign)
                        .Where(ah => ah.EnteredChartId == chartId)
                        .OrderBy(a => a.AngleId)
                        .ToList();

                var angles =
                    chartAngles.Select(
                        an =>
                        new ChartObject
                            {
                                Sign = an.Sign, 
                                SignId = an.SignId, 
                                Degrees = an.Degrees, 
                                Minutes = an.Minutes, 
                                Seconds = an.Seconds, 
                                CelestialObject =
                                    new CelestialObject { CelestialObjectName = an.HouseAngle.AngleName }
                            })
                        .ToList();

                var cusps =
                    cuspHouses.Select(
                        ah =>
                        new ChartObject
                            {
                                Sign = ah.Sign, 
                                SignId = ah.SignId, 
                                Degrees = ah.Degrees, 
                                Minutes = ah.Minutes, 
                                Seconds = ah.Seconds, 
                                CelestialObject =
                                    new CelestialObject { CelestialObjectName = ah.HouseCusp.HouseCuspName }
                            })
                        .ToList();

                ////var angleCusps = this.db.ChartHouses.Where(houses => houses.EnteredChartId == id && houses.HouseAngle.HouseAngleName != "Vertex").ToList();
                var planets =
                    this.db.ChartObjects.Include(ob => ob.Sign)
                        .Where(
                            p =>
                            p.EnteredChartID == chartId
                            && p.CelestialObject.CelestialObjectTypeId
                            == (byte)ChartObject.ObjectTypes.MajorPlanetLuminary && !p.CelestialObject.Draconic)
                        .OrderBy(p => p.CelestialObjectId)
                        .ToList();

                var houseRulers = new List<ChartObject>();

                // House Cusps / Angles (we don't worry as much about Vertex)
                // IF any house/angle is missing, end.
                if (cusps.Count() != 12)
                {
                    return new List<ChartObject>();
                }

                var northNode =
                    this.db.ChartObjects.FirstOrDefault(
                        nn => nn.CelestialObject.CelestialObjectName == "True Node" && nn.EnteredChartID == chartId);

                ChartObject southNode = null;
                if (northNode != null)
                {
                    southNode = new ChartObject
                                    {
                                        EnteredChart = northNode.EnteredChart, 
                                        EnteredChartID = northNode.EnteredChartID, 
                                        Degrees = northNode.Degrees, 
                                        Minutes = northNode.Minutes, 
                                        Seconds = northNode.Seconds, 
                                        SignId = northNode.SignId, 
                                        Sign = null, 
                                        CelestialObject =
                                            new CelestialObject
                                                {
                                                    AllowableOrb =
                                                        northNode.CelestialObject.AllowableOrb, 
                                                    AlternateName = null, 
                                                    CelestialObjectId = 0, 
                                                    CelestialObjectName = "South Node", 
                                                    Draconic = false, 
                                                    CelestialObjectType =
                                                        new CelestialObjectType
                                                            {
                                                                CelestialObjectTypeName
                                                                    =
                                                                    northNode
                                                                    .CelestialObject
                                                                    .CelestialObjectType
                                                                    .CelestialObjectTypeName, 
                                                                CelestialObjectTypeId
                                                                    =
                                                                    northNode
                                                                    .CelestialObject
                                                                    .CelestialObjectType
                                                                    .CelestialObjectTypeId
                                                            }, 
                                                    CelestialObjectTypeId =
                                                        northNode.CelestialObject
                                                        .CelestialObjectTypeId
                                                }, 
                                        CelestialObjectId = northNode.CelestialObjectId, 
                                        Orientation =
                                            new Orientation
                                                {
                                                    OrientationId =
                                                        northNode.Orientation.OrientationId, 
                                                    OrientationAbbreviation =
                                                        northNode.Orientation
                                                        .OrientationAbbreviation
                                                }, 
                                        OrientationId = northNode.OrientationId
                                    };
                    southNode.CelestialObject.CelestialObjectName = "South Node";
                    if (northNode.SignId > 5)
                    {
                        southNode.SignId = (byte)(northNode.SignId - 6);
                    }
                    else
                    {
                        southNode.SignId = (byte)(northNode.SignId + 6);
                    }

                    southNode.Sign = this.db.Signs.Find(southNode.SignId);
                }

                // IF any planet is missing, end.
                if (planets.Count != 10)
                {
                    return new List<ChartObject>();
                }

                for (var i = 0; i < 12; i++)
                {
                    houseRulers.Add(planets.FirstOrDefault(p => p.CelestialObjectId == cusps[i].Sign.ModernRulerId));
                }

                // If the sun is in houses 7-12, it's a day chart.  Otherwise, treat as a night chart.
                var sun = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Sun");
                var moon = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Moon");
                var mercury = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Mercury");
                var venus = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Venus");
                var mars = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Mars");
                var jupiter = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Jupiter");
                var saturn = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Saturn");
                var uranus = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Uranus");
                var neptune = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Neptune");
                var pluto = planets.FirstOrDefault(p => p.CelestialObject.CelestialObjectName == "Pluto");

                var first = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "1st House Cusp");
                var second = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "2nd House Cusp");
                var third = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "3rd House Cusp");
                var fourth = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "4th House Cusp");
                var fifth = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "5th House Cusp");

                ////var sixth = angleCusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "6th House Cusp");
                var seventh = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "7th House Cusp");
                var eighth = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "8th House Cusp");
                var ninth = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "9th House Cusp");

                ////var tenth = cusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "10th House Cusp");
                ////var eleventh = angleCusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "11th House Cusp");
                ////var twelfth = angleCusps.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "12th House Cusp");
                var ascendant = angles.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "Ascendant");
                var midheaven = angles.FirstOrDefault(a => a.CelestialObject.CelestialObjectName == "Midheaven");

                if (ascendant == null || midheaven == null)
                {
                    return new List<ChartObject>();
                }

                var rulerOfFirst = houseRulers[0];
                var rulerOfSecond = houseRulers[1];
                var rulerOfThird = houseRulers[2];

                ////var rulerOfFourth = houseRulers[3];
                var rulerOfFifth = houseRulers[4];

                ////var rulerOfSixth = houseRulers[5];
                ////var rulerOfSeventh = houseRulers[6];
                ////var rulerOfEighth = houseRulers[7];
                ////var rulerOfNinth = houseRulers[8];
                var rulerOfTenth = houseRulers[9];

                ////var rulerOfEleventh = houseRulers[10];
                var rulerOfTwelfth = houseRulers[11];

                var cancerId = this.db.Signs.FirstOrDefault(s => s.SignName == "Cancer").SignId;
                var libraId = this.db.Signs.FirstOrDefault(s => s.SignName == "Libra").SignId;

                var isDayChart = first != null
                                 && (seventh != null
                                     && (sun != null
                                         && ((sun.CalculatedCoordinate > seventh.CalculatedCoordinate)
                                             || (sun.CalculatedCoordinate < first.CalculatedCoordinate))));

                ChartObject partoffortune, partofspirit;

                if (isDayChart)
                {
                    partoffortune = this.NewArabicPart(chartId, "Part of Fortune", first, moon, sun);
                    partofspirit = this.NewArabicPart(chartId, "Part of Spirit", first, sun, moon);
                }
                else
                {
                    partoffortune = this.NewArabicPart(chartId, "Part of Fortune", first, sun, moon);
                    partofspirit = this.NewArabicPart(chartId, "Part of Spirit", first, moon, sun);
                }

                var lastCuspBeforeMoon = cusps.LastOrDefault(x => x.CalculatedCoordinate < moon.CalculatedCoordinate)
                                         ?? cusps.LastOrDefault(
                                             x => x.CalculatedCoordinate < (moon.CalculatedCoordinate + 360M));

                var lastCuspIndex = cusps.IndexOf(lastCuspBeforeMoon);

                var rulerMoonHouse = houseRulers[lastCuspIndex];

                var newParts = new List<ChartObject> { partoffortune, partofspirit };

                if (isDayChart)
                {
                    newParts.Add(this.NewArabicPart(chartId, "Part of Ancestors/Relations", first, mars, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Children", first, saturn, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Danger, Violence, Debt", first, mercury, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Death (Parents)", first, jupiter, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Debt", first, mercury, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Destiny", midheaven, sun, moon));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Fame", first, jupiter, sun));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Father", first, sun, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Grandparents (1)", first, jupiter, second));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Grandparents (2)", first, saturn, second));
                    newParts.Add(
                        this.NewArabicPart(
                            chartId, 
                            "Part of Journeys (Water)", 
                            first, 
                            new ChartObject
                                {
                                    EnteredChartID = chartId, 
                                    SignId = cancerId, 
                                    OrientationId = 1, 
                                    Degrees = 15, 
                                    Minutes = 0, 
                                    Seconds = 0, 
                                    CelestialObject =
                                        new CelestialObject
                                            {
                                                CelestialObjectTypeId = 1, 
                                                AllowableOrb = 1M, 
                                                Draconic = false
                                            }
                                }, 
                            saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Knowledge", first, moon, mercury));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Life, Reincarnation", first, saturn, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Peril", first, eighth, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Real Estate (Land)", first, moon, saturn));
                    newParts.Add(
                        this.NewArabicPart(chartId, "Part of Real Estate (Investment)", first, jupiter, mercury));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Son-in-Laws", first, venus, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Success", first, jupiter, partoffortune));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Surgery", first, saturn, mars));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Victory", first, jupiter, partofspirit));
                }
                else
                {
                    newParts.Add(this.NewArabicPart(chartId, "Part of Ancestors/Relations", first, saturn, mars));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Children", first, jupiter, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Danger, Violence, Debt", first, saturn, mercury));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Death (Parents)", first, saturn, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Debt", first, saturn, mercury));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Destiny", midheaven, moon, sun));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Fame", first, sun, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Father", first, saturn, sun));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Grandparents (1)", first, second, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Grandparents (2)", first, second, saturn));
                    newParts.Add(
                        this.NewArabicPart(
                            chartId, 
                            "Part of Journeys (Water)", 
                            first, 
                            saturn, 
                            new ChartObject
                                {
                                    EnteredChartID = chartId, 
                                    SignId = cancerId, 
                                    OrientationId = 1, 
                                    Degrees = 15, 
                                    Minutes = 0, 
                                    Seconds = 0, 
                                    CelestialObject =
                                        new CelestialObject
                                            {
                                                CelestialObjectTypeId = 1, 
                                                AllowableOrb = 1M, 
                                                Draconic = false
                                            }
                                }));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Knowledge", first, mercury, moon));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Life, Reincarnation", first, jupiter, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Peril", first, saturn, eighth));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Real Estate (Land)", first, saturn, moon));
                    newParts.Add(
                        this.NewArabicPart(chartId, "Part of Real Estate (Investment)", first, mercury, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Son-in-Laws", first, saturn, venus));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Success", first, partoffortune, jupiter));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Surgery", first, mars, saturn));
                    newParts.Add(this.NewArabicPart(chartId, "Part of Victory", first, partofspirit, jupiter));
                }

                newParts.Add(this.NewArabicPart(chartId, "Part of Ability", first, mars, rulerOfFirst));
                newParts.Add(this.NewArabicPart(chartId, "Part of Abundance", first, sun, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Accident", first, saturn, mars));
                newParts.Add(this.NewArabicPart(chartId, "Part of Accomplishment", first, sun, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Action/Reasoning", first, mars, mercury));
                if (southNode != null)
                {
                    newParts.Add(this.NewArabicPart(chartId, "Part of Addiction", first, southNode, neptune));
                }

                newParts.Add(this.NewArabicPart(chartId, "Part of Administrators", first, mars, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Agriculture", first, saturn, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Allegiance", first, saturn, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Ancestral Heritage", first, moon, eighth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Armies", first, saturn, mars));
                newParts.Add(this.NewArabicPart(chartId, "Part of Art", first, venus, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Assassination (1)", first, rulerOfTwelfth, neptune));
                newParts.Add(this.NewArabicPart(chartId, "Part of Assassination (2)", mars, neptune, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Assurance", first, jupiter, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Astrology", first, uranus, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Bad Luck", first, partoffortune, partofspirit));
                newParts.Add(this.NewArabicPart(chartId, "Part of Bankruptcy (1)", jupiter, neptune, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Bankruptcy (2)", jupiter, jupiter, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Beauty", first, venus, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Benific Change", first, pluto, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Benevolence", first, jupiter, pluto));
                newParts.Add(this.NewArabicPart(chartId, "Part of Business Partnerships", first, seventh, rulerOfTenth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Cancer", first, neptune, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Catastrophe (1)", first, uranus, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Catastrophe (2)", first, uranus, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Caution", first, neptune, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Commerce (1)", first, mercury, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Commerce (2)", first, mars, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Controversy", first, jupiter, mars));
                newParts.Add(this.NewArabicPart(chartId, "Part of Corruptness", first, neptune, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Curiosity", first, moon, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Damage", first, neptune, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Daughters", first, venus, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Death", first, eighth, moon));
                newParts.Add(
                    this.NewArabicPart(chartId, "Part of Desire, Sexual Attraction", first, fifth, rulerOfFifth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Destruction", first, mars, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Disease", first, mars, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Divorce (1)", first, venus, seventh));
                newParts.Add(this.NewArabicPart(chartId, "Part of Divorce (2)", first, seventh, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Eccentricity", first, mercury, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Energy, Sex Drive", first, pluto, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Expected Birth (1)", first, rulerMoonHouse, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Expected Birth (2)", first, venus, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Famous Friends", first, partoffortune, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Fascination", first, venus, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Fatality", first, saturn, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Fate (Karma)", first, saturn, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Fraud", first, neptune, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Friends (1)", first, moon, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Friends (2)", first, mercury, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Friends (3)", first, moon, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Genius", first, sun, neptune));
                newParts.Add(this.NewArabicPart(chartId, "Part of Guidance", first, neptune, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Happiness", first, uranus, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Homosexuality", first, mars, uranus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Horsemanship", first, moon, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Identity", first, saturn, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Imprisonment", first, sun, neptune));
                newParts.Add(this.NewArabicPart(chartId, "Part of Increase", first, jupiter, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Inheritance (1)", first, moon, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Inheritance (2)", first, jupiter, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Journeys (Air)", first, uranus, ninth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Journeys (Land)", first, ninth, ninth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Kings, Rulers", first, moon, mercury));
                newParts.Add(this.NewArabicPart(chartId, "Part of Love", first, venus, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Lovers", mars, venus, fifth));
                newParts.Add(this.NewArabicPart(chartId, "Part of Luck", first, moon, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Marriage", first, seventh, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Marriage of Woman (1)", first, saturn, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Marriage of Woman (2)", first, mars, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Marriage of Man (1)", first, venus, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Marriage of Man (2)", first, venus, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Mother", first, moon, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Partners", first, seventh, venus));
                newParts.Add(this.NewArabicPart(chartId, "Part of Possessions", first, second, rulerOfSecond));
                newParts.Add(this.NewArabicPart(chartId, "Part of Secret Enemies", first, moon, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Short Journeys", first, third, rulerOfThird));
                newParts.Add(this.NewArabicPart(chartId, "Part of Siblings", first, saturn, jupiter));
                newParts.Add(this.NewArabicPart(chartId, "Part of Sickness", first, mars, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Sons", fourth, moon, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Success (Investment)", first, venus, saturn));
                newParts.Add(this.NewArabicPart(chartId, "Part of Suicide(1)", first, eighth, neptune));
                newParts.Add(this.NewArabicPart(chartId, "Part of Suicide(2)", first, jupiter, neptune));
                newParts.Add(this.NewArabicPart(chartId, "Part of Tragedy", first, saturn, sun));
                newParts.Add(this.NewArabicPart(chartId, "Part of Unusual Events", first, uranus, moon));
                newParts.Add(this.NewArabicPart(chartId, "Part of Weddings, Legal Contracts", ninth, third, venus));
                newParts.Add(
                    this.NewArabicPart(
                        chartId, 
                        "Part of Widowhood", 
                        first, 
                        new ChartObject
                            {
                                EnteredChartID = chartId, 
                                SignId = libraId, 
                                OrientationId = 1, 
                                Degrees = 8, 
                                Minutes = 50, 
                                Seconds = 0, 
                                CelestialObject =
                                    new CelestialObject
                                        {
                                            CelestialObjectTypeId = 1, 
                                            AllowableOrb = 1M, 
                                            Draconic = false
                                        }
                            }, 
                        neptune));

                return newParts;
            }
            catch
            {
                return new List<ChartObject>();
            }
        }

        /// <summary>
        /// Gets the conjunct chart objects.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// A JSON array of chart objects and their aspects to the specified chart object.
        /// </returns>
        [CanBeNull]
        public JsonResult GetAspectChartObjects(
            int id, 
            int chartId, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            bool stars, 
            byte houseSystemId)
        {
            if (id <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = this.db.ChartObjects.Find(id);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == thisObject.EnteredChartID && x.ChartObjectId != id)
                    .Where(
                        objects =>
                        (!objects.CelestialObject.Draconic)
                        && (objects.CelestialObject.CelestialObjectTypeId
                            != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                        && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                        && ((!asteroids
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                            || asteroids)
                        && ((!stars
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                            || stars))
                    .ToList()
                    .Union(this.GetAngleChartObjects(thisObject.EnteredChartID))
                    .Union(this.GetArabicPartChartObjects(thisObject.EnteredChartID, houseSystemId, arabic))
                    .Union(
                        this.GetDraconicChartObjects(
                            thisObject.EnteredChartID, 
                            draconic, 
                            arabic, 
                            asteroids, 
                            houseSystemId))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.AspectId).ToList();

            var aspectObjectLists = this.GetAspectObjectLists(thisChart, thisObject, chartId, houseSystemId); 

            var thisList = new List<object>();

            for (var i = 0; i < aspectList.Count; i++)
            {
                thisList.Add(
                    new
                        {
                            aspectList[i].AspectId,
                            aspectList[i].AspectName,
                            aspectList[i].HtmlTextCssClass,
                            aspectList = aspectObjectLists[i]
                        });
            }

            return this.Json(thisList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect chart objects for angle.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="angleName">Name of the angle.</param>
        /// <param name="angleCoordinates">The angle coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// a JSON object containing all of the objects that aspect the given Arabic Part.
        /// </returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetAspectChartObjectsForAngle(
            int chartId, 
            [CanBeNull] string angleName, 
            [CanBeNull] string angleCoordinates, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            bool stars, 
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(angleName) || string.IsNullOrEmpty(angleCoordinates) || chartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var angles = this.GetAngleChartObjects(chartId);

            if (angles.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = angles.FirstOrDefault(ap => ap.CelestialObject.CelestialObjectName == angleName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == chartId)
                    .Where(
                        objects =>
                        (!objects.CelestialObject.Draconic)
                        && (objects.CelestialObject.CelestialObjectTypeId
                            != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                        && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                        && ((!asteroids
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                            || asteroids)
                        && ((!stars
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                            || stars))
                    .ToList()
                    .Union(angles.Where(x => x.CelestialObject.CelestialObjectName != angleName))
                    .Union(this.GetArabicPartChartObjects(chartId, houseSystemId, arabic))
                    .Union(this.GetDraconicChartObjects(chartId, draconic, arabic, asteroids, houseSystemId))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.AspectId).ToList();

            var aspectObjectLists = this.GetAspectObjectLists(thisChart, thisObject, chartId, houseSystemId);

            var thisList = new List<object>();

            for (var i = 0; i < aspectList.Count; i++)
            {
                thisList.Add(
                    new
                        {
                            aspectList[i].AspectId,
                            aspectList[i].AspectName,
                            aspectList[i].HtmlTextCssClass,
                            aspectList = aspectObjectLists[i]
                        });
            }

            return this.Json(thisList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect chart objects for arabic part.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="arabicPartName">Name of the arabic part.</param>
        /// <param name="arabicPartCoordinates">The arabic part coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// a JSON object containing all of the objects that aspect the given Arabic Part.
        /// </returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetAspectChartObjectsForArabicPart(
            int chartId, 
            [CanBeNull] string arabicPartName, 
            [CanBeNull] string arabicPartCoordinates, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            bool stars, 
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(arabicPartName) || string.IsNullOrEmpty(arabicPartCoordinates) || chartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var arabicParts = this.GetArabicPartChartObjects(chartId, houseSystemId, arabic);

            if (arabicParts.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = arabicParts.FirstOrDefault(ap => ap.CelestialObject.CelestialObjectName == arabicPartName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == chartId)
                    .Where(
                        objects =>
                        (!objects.CelestialObject.Draconic)
                        && (objects.CelestialObject.CelestialObjectTypeId
                            != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                        && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                        && ((!asteroids
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                            || asteroids)
                        && ((!stars
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                            || stars))
                    .ToList()
                    .Union(this.GetAngleChartObjects(chartId))
                    .Union(arabicParts.Where(ap => ap.CelestialObject.CelestialObjectName != arabicPartName))
                    .Union(this.GetDraconicChartObjects(chartId, draconic, arabic, asteroids, houseSystemId))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.AspectId).ToList();

            var aspectObjectLists = this.GetAspectObjectLists(thisChart, thisObject, chartId, houseSystemId);

            var thisList = new List<object>();

            for (var i = 0; i < aspectList.Count; i++)
            {
                thisList.Add(
                    new { aspectList[i].AspectId, aspectList[i].AspectName, aspectList[i].HtmlTextCssClass, aspectList = aspectObjectLists[i] });
            }

            return this.Json(thisList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect chart objects for draconic object.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="draconicName">Name of the draconic.</param>
        /// <param name="draconicCoordinates">The draconic coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// a JSON object containing all of the objects that aspect the given Draconic Object.
        /// </returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetAspectChartObjectsForDraconicObject(
            int chartId, 
            [CanBeNull] string draconicName, 
            [CanBeNull] string draconicCoordinates, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            bool stars, 
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(draconicName) || string.IsNullOrEmpty(draconicCoordinates) || chartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var draconicObjects = this.GetDraconicChartObjects(chartId, draconic, arabic, asteroids, houseSystemId);

            if (draconicObjects.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = draconicObjects.FirstOrDefault(
                ap => ap.CelestialObject.CelestialObjectName == draconicName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == chartId)
                    .Where(
                        objects =>
                        (!objects.CelestialObject.Draconic)
                        && (objects.CelestialObject.CelestialObjectTypeId
                            != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                        && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                        && ((!asteroids
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                            || asteroids)
                        && ((!stars
                             && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                            || stars))
                    .ToList()
                    .Union(this.GetAngleChartObjects(chartId))
                    .Union(this.GetArabicPartChartObjects(chartId, houseSystemId, arabic))
                    .Union(draconicObjects.Where(ap => ap.CelestialObject.CelestialObjectName != draconicName))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.AspectId).ToList();

            var aspectObjectLists = this.GetAspectObjectLists(thisChart, thisObject, chartId, houseSystemId);

            var thisList = new List<object>();

            for (var i = 0; i < aspectList.Count; i++)
            {
                thisList.Add(
                    new { aspectList[i].AspectId, aspectList[i].AspectName, aspectList[i].HtmlTextCssClass, aspectList = aspectObjectLists[i] });
            }

            return this.Json(thisList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect object list.
        /// </summary>
        /// <param name="thisChart">The this chart.</param>
        /// <param name="thisObject">The this object.</param>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>The Aspect Listing as an array.</returns>
        [NotNull]
        public IEnumerable<object>[] GetAspectObjectLists([NotNull] IOrderedEnumerable<ChartObject> thisChart, [CanBeNull] ChartObject thisObject, int chartId, byte houseSystemId)
        {
            if (thisChart == null)
            {
                throw new ArgumentNullException("thisChart");
            }

            return new[]
                       {
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsConjunct(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsOpposite(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSquare(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSemisquare(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSesquiquadrate(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsTrine(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSextile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsQuincunx(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsQuintile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsBiquintile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSemiSextile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsSeptile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsBiseptile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsTriseptile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsNovile(thisObject)).ToList(),
                               chartId,
                               houseSystemId),
                           this.CreateAspectChartEnumerable(
                               thisObject,
                               thisChart.Where(x => thisObject != null && x.IsDecile(thisObject)).ToList(),
                               chartId,
                               houseSystemId)
                       };
        }

        /// <summary>
        /// Gets the automatic complete for asteroid fixed star.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="enteredName">Name of the entered.</param>
        /// <returns>
        /// The autocomplete for a given Asteroid/Fixed Star field.
        /// </returns>
        [CanBeNull]
        public JsonResult GetAutoCompleteForAsteroidFixedStar(int chartId, [NotNull] string enteredName)
        {
            if (enteredName == null)
            {
                throw new ArgumentNullException("enteredName");
            }

            return
                this.Json(
                    this.db.AutoCompleteForEnteredChart(enteredName, chartId)
                        .Select(x => new { x.CelestialObjectId, x.CelestialObjectName, x.ObjectExists }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the details angle listing.
        /// </summary>
        /// <param name="chartId">
        /// The identifier.
        /// </param>
        /// <returns>
        /// JSON listing of the chart angles for the specified chart ID.
        /// </returns>
        [CanBeNull]
        public JsonResult GetDetailsAngleListing(int chartId)
        {
            var signs = this.db.Signs.Include(x => x.Element).ToList();

            var chartAngles =
                this.db.ChartAngles.Where(angles => angles.EnteredChartId == chartId)
                    .Select(
                        x =>
                        new
                            {
                                x.ChartAngleId, 
                                x.HouseAngle.AngleName, 
                                x.Degrees, 
                                x.Minutes, 
                                x.Seconds, 
                                x.Sign.SignAbbreviation, 
                                x.Sign.SignId, 
                                x.Sign.Element.HtmlTextCssClass, 
                                x.AngleId
                            })
                    .ToList();

            var vertex = chartAngles.FirstOrDefault(x => x.AngleId == 0);

            if (vertex != null)
            {
                var antivertex =
                    new
                        {
                            ChartAngleId = 0, 
                            AngleName = "Antivertex", 
                            vertex.Degrees, 
                            vertex.Minutes, 
                            vertex.Seconds, 
                            SignAbbreviation =
                                vertex.SignId < 6
                                    ? signs[vertex.SignId + 6].SignAbbreviation
                                    : signs[vertex.SignId - 6].SignAbbreviation, 
                            SignId = vertex.SignId < 6 ? (byte)(vertex.SignId + 6) : (byte)(vertex.SignId - 6), 
                            HtmlTextCssClass =
                                vertex.SignId < 6
                                    ? signs[vertex.SignId + 6].Element.HtmlTextCssClass
                                    : signs[vertex.SignId - 6].Element.HtmlTextCssClass, 
                            AngleId = (byte)3
                        };

                chartAngles.Add(antivertex);
            }

            var ascendant = chartAngles.FirstOrDefault(x => x.AngleId == 1);

            if (ascendant != null)
            {
                var descendant =
                    new
                        {
                            ChartAngleId = 0, 
                            AngleName = "Descendant", 
                            ascendant.Degrees, 
                            ascendant.Minutes, 
                            ascendant.Seconds, 
                            SignAbbreviation =
                                ascendant.SignId < 6
                                    ? signs[ascendant.SignId + 6].SignAbbreviation
                                    : signs[ascendant.SignId - 6].SignAbbreviation, 
                            SignId = ascendant.SignId < 6 ? (byte)(ascendant.SignId + 6) : (byte)(ascendant.SignId - 6), 
                            HtmlTextCssClass =
                                ascendant.SignId < 6
                                    ? signs[ascendant.SignId + 6].Element.HtmlTextCssClass
                                    : signs[ascendant.SignId - 6].Element.HtmlTextCssClass, 
                            AngleId = (byte)4
                        };

                chartAngles.Add(descendant);
            }

            var midheaven = chartAngles.FirstOrDefault(x => x.AngleId == 2);

            if (midheaven != null)
            {
                var imumCoeli =
                    new
                        {
                            ChartAngleId = 0, 
                            AngleName = "Imum Coeli", 
                            midheaven.Degrees, 
                            midheaven.Minutes, 
                            midheaven.Seconds, 
                            SignAbbreviation =
                                midheaven.SignId < 6
                                    ? signs[midheaven.SignId + 6].SignAbbreviation
                                    : signs[midheaven.SignId - 6].SignAbbreviation, 
                            SignId = midheaven.SignId < 6 ? (byte)(midheaven.SignId + 6) : (byte)(midheaven.SignId - 6), 
                            HtmlTextCssClass =
                                midheaven.SignId < 6
                                    ? signs[midheaven.SignId + 6].Element.HtmlTextCssClass
                                    : signs[midheaven.SignId - 6].Element.HtmlTextCssClass, 
                            AngleId = (byte)5
                        };

                chartAngles.Add(imumCoeli);
            }

            return this.Json(chartAngles, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Details the chart listing.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// JSON listing of the chart objects for the specified chart ID.
        /// </returns>
        [CanBeNull]
        public JsonResult GetDetailsChartListing(
            int id, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            bool stars, 
            byte houseSystemId)
        {
            // Get the houses if available.
            var chartHouses =
                this.db.ChartHouses.Where(
                    houses =>
                    houses.EnteredChartId == id && houses.HouseSystemId == houseSystemId && houses.HouseId != 0)
                    .ToList()
                    .OrderBy(houses => houses.CoordinateInSeconds)
                    .Select(x => new { x.Degrees, x.Minutes, x.Seconds, x.Sign.SignId, x.HouseId })
                    .ToList();

            var chartDetails =
                this.db.ChartObjects.Where(
                    objects =>
                    (objects.EnteredChartID == id) && (!objects.CelestialObject.Draconic)
                    && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                    && (objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                    && ((!asteroids
                         && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                        || asteroids)
                    && ((!stars
                         && objects.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                        || stars))
                    .ToList()
                    .Union(this.GetAngleChartObjects(id))
                    .Union(this.GetArabicPartChartObjects(id, houseSystemId, arabic))
                    .Union(this.GetDraconicChartObjects(id, draconic, arabic, asteroids, houseSystemId))
                    .OrderBy(objects => objects.CalculatedCoordinate)
                    .ToList()
                    .Select(
                        x =>
                        new
                            {
                                x.ChartObjectId, 
                                x.CelestialObject.CelestialObjectName, 
                                x.Sign.SignAbbreviation, 
                                x.Sign.Element.HtmlTextCssClass, 
                                x.Degrees, 
                                x.Minutes, 
                                x.Seconds, 
                                x.Orientation.OrientationAbbreviation, 
                                x.CelestialObject.CelestialObjectType.CelestialObjectTypeName, 
                                x.CelestialObject.Draconic, 
                                House =
                            (chartHouses.Count > 0)
                                ? (chartHouses.LastOrDefault(
                                    h =>
                                    (((((h.SignId * 30) + h.Degrees) * 3600) + (h.Minutes * 60) + h.Seconds)
                                     <= ((((x.SignId * 30) + x.Degrees) * 3600) + (x.Minutes * 60) + x.Seconds)))
                                   ?? chartHouses.Last()).HouseId
                                : 0,
                                x.AngleId
                            })
                    .ToList();

            return this.Json(chartDetails, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the details draconic angle listing.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>A listing of Draconic Chart Angles.</returns>
        [CanBeNull]
        public JsonResult GetDetailsDraconicAngleListing(int chartId)
        {
            var northNode =
                this.db.ChartObjects.FirstOrDefault(
                    x => x.CelestialObject.CelestialObjectName == "True Node" && x.EnteredChartID == chartId);

            var signs = this.db.Signs.Include(x => x.Element).ToList();

            var baseAngles = this.db.ChartAngles.Where(angles => angles.EnteredChartId == chartId).ToList();

            var draconicBase = baseAngles.Select(ba => ba.GetDraconicChartAngle(northNode)).ToList();

            var draconicAngles =
                draconicBase.Select(
                    x =>
                    new
                        {
                            x.ChartAngleId,
                            x.HouseAngle.AngleName,
                            x.Degrees,
                            x.Minutes,
                            x.Seconds,
                            x.Sign.SignAbbreviation,
                            x.Sign.SignId,
                            x.Sign.Element.HtmlTextCssClass,
                            x.AngleId
                        }).ToList();
            
            var vertex = draconicAngles.FirstOrDefault(x => x.AngleId == 0);

            if (vertex != null)
            {
                var antivertex =
                    new
                        {
                            ChartAngleId = 0,
                            AngleName = "Dr. Antivertex",
                            vertex.Degrees,
                            vertex.Minutes,
                            vertex.Seconds,
                            SignAbbreviation =
                                vertex.SignId < 6
                                    ? signs[vertex.SignId + 6].SignAbbreviation
                                    : signs[vertex.SignId - 6].SignAbbreviation,
                            SignId = vertex.SignId < 6 ? (byte)(vertex.SignId + 6) : (byte)(vertex.SignId - 6),
                            HtmlTextCssClass =
                                vertex.SignId < 6
                                    ? signs[vertex.SignId + 6].Element.HtmlTextCssClass
                                    : signs[vertex.SignId - 6].Element.HtmlTextCssClass,
                            AngleId = (byte)3
                        };

                draconicAngles.Add(antivertex);
            }

            var ascendant = draconicAngles.FirstOrDefault(x => x.AngleId == 1);

            if (ascendant != null)
            {
                var descendant =
                    new
                        {
                            ChartAngleId = 0,
                            AngleName = "Dr. Descendant",
                            ascendant.Degrees,
                            ascendant.Minutes,
                            ascendant.Seconds,
                            SignAbbreviation =
                                ascendant.SignId < 6
                                    ? signs[ascendant.SignId + 6].SignAbbreviation
                                    : signs[ascendant.SignId - 6].SignAbbreviation,
                            SignId = ascendant.SignId < 6 ? (byte)(ascendant.SignId + 6) : (byte)(ascendant.SignId - 6),
                            HtmlTextCssClass =
                                ascendant.SignId < 6
                                    ? signs[ascendant.SignId + 6].Element.HtmlTextCssClass
                                    : signs[ascendant.SignId - 6].Element.HtmlTextCssClass,
                            AngleId = (byte)4
                        };

                draconicAngles.Add(descendant);
            }

            var midheaven = draconicAngles.FirstOrDefault(x => x.AngleId == 2);

            if (midheaven != null)
            {
                var imumCoeli =
                    new
                        {
                            ChartAngleId = 0,
                            AngleName = "Dr. Imum Coeli",
                            midheaven.Degrees,
                            midheaven.Minutes,
                            midheaven.Seconds,
                            SignAbbreviation =
                                midheaven.SignId < 6
                                    ? signs[midheaven.SignId + 6].SignAbbreviation
                                    : signs[midheaven.SignId - 6].SignAbbreviation,
                            SignId = midheaven.SignId < 6 ? (byte)(midheaven.SignId + 6) : (byte)(midheaven.SignId - 6),
                            HtmlTextCssClass =
                                midheaven.SignId < 6
                                    ? signs[midheaven.SignId + 6].Element.HtmlTextCssClass
                                    : signs[midheaven.SignId - 6].Element.HtmlTextCssClass,
                            AngleId = (byte)5
                        };

                draconicAngles.Add(imumCoeli);
            }

            return this.Json(draconicAngles, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the details house listing.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// JSON listing of the chart houses for the specified chart ID.
        /// </returns>
        [CanBeNull]
        public JsonResult GetDetailsDraconicHouseListing(int chartId, byte houseSystemId)
        {
            var northNode =
                this.db.ChartObjects.FirstOrDefault(
                    x => x.CelestialObject.CelestialObjectName == "True Node" && x.EnteredChartID == chartId);

            var draconicBase = new List<ChartHouse>();

            var chartHouses =
                this.db.ChartHouses.Where(
                    houses => houses.EnteredChartId == chartId && houses.HouseSystemId == houseSystemId).ToList();

            chartHouses.ForEach(x => draconicBase.Add(x.GetDraconicChartHouse(northNode)));

            var draconicHouses =
                draconicBase.Select(
                    x =>
                    new
                        {
                            x.ChartHouseId, 
                            x.Degrees, 
                            x.Minutes, 
                            x.Seconds, 
                            x.Sign.SignAbbreviation, 
                            x.Sign.SignId, 
                            x.Sign.Element.HtmlTextCssClass, 
                            x.HouseId
                        }).ToList();

            return this.Json(draconicHouses, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the details house listing.
        /// </summary>
        /// <param name="chartId">The identifier.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// JSON listing of the chart houses for the specified chart ID.
        /// </returns>
        [CanBeNull]
        public JsonResult GetDetailsHouseListing(int chartId, byte houseSystemId)
        {
            var chartHouses =
                this.db.ChartHouses.Where(
                    houses => houses.EnteredChartId == chartId && houses.HouseSystemId == houseSystemId)
                    .Select(
                        x =>
                        new
                            {
                                x.ChartHouseId, 
                                x.Degrees, 
                                x.Minutes, 
                                x.Seconds, 
                                x.Sign.SignAbbreviation, 
                                x.Sign.SignId, 
                                x.Sign.Element.HtmlTextCssClass, 
                                x.HouseId
                            })
                    .ToList();

            return this.Json(chartHouses, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the draconic chart objects.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// A listing of Draconic Chart objects.
        /// </returns>
        [NotNull]
        public List<ChartObject> GetDraconicChartObjects(
            int chartId, 
            bool draconic, 
            bool arabic, 
            bool asteroids, 
            byte houseSystemId)
        {
            if (chartId <= 0 || !draconic)
            {
                return new List<ChartObject>();
            }

            var northNode =
                this.db.ChartObjects.FirstOrDefault(
                    x => x.CelestialObject.CelestialObjectName == "True Node" && x.EnteredChartID == chartId);

            if (northNode == null)
            {
                return new List<ChartObject>();
            }

            ////&& (!x.CelestialObject.AlternateName.Contains("House Cusp") || x.CelestialObject.AlternateName == null)
            var baseObjects =
                this.db.ChartObjects.Where(
                    x =>
                    x.EnteredChartID == chartId
                    && (x.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.AngleHouseCusp)
                    && (x.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.FixedStar)
                    && (x.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.ArabicPart)
                    && (x.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Nodes)
                    && ((!asteroids && x.CelestialObject.CelestialObjectTypeId != (byte)ChartObject.ObjectTypes.Asteroid)
                        || asteroids) && !x.CelestialObject.Draconic)
                    .ToList()
                    .Union(this.GetAngleChartObjects(chartId))
                    .Union(this.GetArabicPartChartObjects(chartId, houseSystemId, arabic))
                    .ToList();

            ////var signList = this.db.Signs.ToList();
            var draconicObjects = new List<ChartObject>();

            baseObjects.ForEach(x => draconicObjects.Add(x.GetDraconicChartObject(northNode)));

            return draconicObjects;
        }

        /// <summary>
        /// Gets the existence of planets and secondary objects.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>Whether all planets and secondary objects exist for the given chart.</returns>
        [CanBeNull]
        public JsonResult GetExistenceOfPlanetsAndSecondaries(int chartId)
        {
            return
                this.Json(
                    this.db.PrimariesAndSecondariesExistInEnteredChart(chartId)
                        .Select(x => new { x.AllPlanetsAndLuminariesExist, x.AllSecondaryObjectsExist }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the full autocomplete data for asteroids and fixed stars.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>
        /// The full list of Asteroids and Fixed Stars to autocomplete.
        /// </returns>
        [CanBeNull]
        public JsonResult GetFullAutoCompleteForAsteroidFixedStar(int chartId)
        {
            return
                this.Json(
                    this.db.CelestialObjects.Include(x => x.ChartObjects)
                        .Where(
                            x =>
                            x.CelestialObjectTypeId == (byte)ChartObject.ObjectTypes.Asteroid
                            || x.CelestialObjectTypeId == (byte)ChartObject.ObjectTypes.FixedStar)
                        .Select(x => new { x.CelestialObjectName, x.CelestialObjectId }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets if planets exist.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>A list of planet names and whether they exist in the chart.</returns>
        [CanBeNull]
        public JsonResult GetIfPlanetsExist(int chartId)
        {
            ////var existsList =
            ////    this.db.CelestialObjects.Include(x => x.ChartObjects).Where(
            ////        x => x.CelestialObjectTypeId == (byte)ChartObject.ObjectTypes.MajorPlanetLuminary)
            ////        .Select(x => new { PlanetId = x.CelestialObjectId, Planet = x.CelestialObjectName, DoesExist = false });

            return
                this.Json(
                    this.db.PlanetsExistInEnteredChart(chartId)
                        .Select(x => new { x.CelestialObjectId, x.CelestialObjectName, x.ObjectExists }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets if secondary objects exist.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>A list of secondary object names and whether they exist in the chart.</returns>
        [CanBeNull]
        public JsonResult GetIfSecondaryObjectsExist(int chartId)
        {
            ////var existsList =
            ////    this.db.CelestialObjects.Include(x => x.ChartObjects).Where(
            ////        x => x.CelestialObjectTypeId == (byte)ChartObject.ObjectTypes.MajorPlanetLuminary)
            ////        .Select(x => new { PlanetId = x.CelestialObjectId, Planet = x.CelestialObjectName, DoesExist = false });

            return
                this.Json(
                    this.db.SecondaryObjectsExistInEnteredChart(chartId)
                        .Select(x => new { x.CelestialObjectId, x.CelestialObjectName, x.ObjectExists }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the orientation list.
        /// </summary>
        /// <returns>
        /// A JSON array of orientations for the drop down list.
        /// </returns>
        [CanBeNull]
        public JsonResult GetOrientationsList()
        {
            var signs = this.db.Orientations.Select(x => new { x.OrientationId, x.OrientationAbbreviation }).ToList();
            return this.Json(signs, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the conjunct chart objects.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// A JSON array of chart objects and their aspects to the specified chart object.
        /// </returns>
        [CanBeNull]
        public JsonResult GetSelectedAspectChartObject(int id)
        {
            var thisObject =
                this.db.ChartObjects.Where(t => t.ChartObjectId == id)
                    .Select(
                        x =>
                        new
                            {
                                x.EnteredChartID, 
                                x.ChartObjectId, 
                                x.CelestialObjectId,
                                CelestialObjectName =
                            (x.CelestialObject.CelestialObjectTypeId == (byte)ChartObject.ObjectTypes.AngleHouseCusp
                             && x.CelestialObject.AlternateName != null)
                                ? x.CelestialObject.AlternateName
                                : x.CelestialObject.CelestialObjectName, 
                                x.Sign.SignAbbreviation, 
                                x.Sign.Element.HtmlTextCssClass, 
                                x.Sign.SignId, 
                                x.Degrees, 
                                x.Minutes, 
                                x.Seconds, 
                                x.OrientationId, 
                                x.Orientation.OrientationAbbreviation, 
                                x.CelestialObject.CelestialObjectType.CelestialObjectTypeName, 
                                x.CelestialObject.Draconic
                            })
                    .ToList()
                    .FirstOrDefault();

            return this.Json(thisObject, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Gets the signs list.
        /// </summary>
        /// <returns>A JSON array of signs for the drop-down-list.</returns>
        [CanBeNull]
        public JsonResult GetSignsList()
        {
            var signs = this.db.Signs.Select(x => new { x.SignId, x.SignAbbreviation }).ToList();
            return this.Json(signs, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Gets the Entered Charts listing.
        /// </summary>
        /// <returns>The Index Page for Entered Charts.</returns>
        [NotNull]
        public async Task<ActionResult> Index()
        {
            var enteredCharts = this.db.EnteredCharts.Include(e => e.ChartType);
            return this.View(await enteredCharts.ToListAsync());
        }

        /// <summary>
        /// Gets the entered charts listing.
        /// </summary>
        /// <param name="pageNum">The page number.</param>
        /// <param name="entriesPerPage">The entries per page.</param>
        /// <returns>A listing of Entered Charts for the specified page.</returns>
        [CanBeNull]
        public JsonResult GetEnteredChartsListing(int pageNum, int entriesPerPage)
        {
            var enteredCharts = this.db.EnteredCharts.Include(e => e.ChartType);

            var page =
                enteredCharts.OrderBy(x => x.ChartType.ChartTypeId)
                    .ThenBy(x => x.SubjectName)
                    .ThenBy(x => x.OriginDateTimeUnknown)
                    .ThenBy(x => x.OriginDateTime)
                    .Select(x => x)
                    .Skip((pageNum - 1) * entriesPerPage)
                    .Take(entriesPerPage)
                    .GroupBy(x => new { Total = enteredCharts.Count() })
                    .FirstOrDefault();

            var total = (page != null) ? page.Key.Total : 0;

            var listing = (page != null)
                              ? page.Select(
                                  x =>
                                  new
                                      {
                                          x.SubjectName,
                                          x.SubjectLocation,
                                          x.OriginDateTimeString,
                                          x.ChartType.ChartTypeName,
                                          x.EnteredChartId
                                      })
                              : null;

            var returnObj = new object[] { listing, Math.Ceiling((decimal)total / entriesPerPage) };

            ////return this.Json(aspectInterpretations, JsonRequestBehavior.AllowGet);

            return this.Json(returnObj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// News the arabic part.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="baseChartObject">The base chart object.</param>
        /// <param name="addChartObject">The add chart object.</param>
        /// <param name="subtractChartObject">The subtract chart object.</param>
        /// <returns>
        /// A new Arabic Part chart object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">name is null</exception>
        [CanBeNull]
        public ChartObject NewArabicPart(
            int id, 
            [NotNull] string name, 
            [CanBeNull] ChartObject baseChartObject, 
            [CanBeNull] ChartObject addChartObject, 
            [CanBeNull] ChartObject subtractChartObject)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if ((baseChartObject == null) || (addChartObject == null) || (subtractChartObject == null))
            {
                return null;
            }

            const int SecondsInMinutes = 60;
            const int MinutesInDegrees = 60;
            const int DegreesInSign = 30;
            const int SignsInChart = 12;

            ////var diff = baseChartObject.CalculatedCoordinate + addChartObject.CalculatedCoordinate - subtractChartObject.CalculatedCoordinate;
            var diff = baseChartObject.CoordinateInSeconds + addChartObject.CoordinateInSeconds
                       - subtractChartObject.CoordinateInSeconds;

            // There is a risk of the calculation going out of bounds.  Adjust.
            while (diff < 0)
            {
                ////diff += 360;
                diff += SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes;
            }

            ////while (diff > 360M)
            while (diff > SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes)
            {
                ////diff %= 360M;
                diff %= SignsInChart * DegreesInSign * MinutesInDegrees * SecondsInMinutes;
            }

            ////var rawInSign = decimal.Round((diff % 30M) * 3600);
            ////var sign = (byte)((int)diff / 30);
            ////var sec = (int)rawInSign;
            ////var deg = sec / 3600;
            ////sec %= 3600;
            ////var min = sec / 60;
            ////sec %= 60;
            var sign = (byte)(diff / (DegreesInSign * MinutesInDegrees * SecondsInMinutes));

            var deg = (diff / (MinutesInDegrees * SecondsInMinutes)) % DegreesInSign;
            var min = (diff / SecondsInMinutes) % MinutesInDegrees;
            var sec = diff % SecondsInMinutes;

            var newPart = new ChartObject
                              {
                                  EnteredChartID = id, 
                                  CelestialObject =
                                      new CelestialObject
                                          {
                                              CelestialObjectName = name, 
                                              AllowableOrb = 1M, 
                                              CelestialObjectTypeId = 3, 
                                              CelestialObjectType =
                                                  this.db.CelestialObjectTypes.Find(3), 
                                              Draconic =
                                                  baseChartObject.CelestialObject.Draconic
                                          }, 
                                  OrientationId = 1, 
                                  Orientation = this.db.Orientations.Find(1), 
                                  SignId = sign, 
                                  Sign = this.db.Signs.Find(sign), 
                                  Degrees = (byte)deg, 
                                  Minutes = (byte)min, 
                                  Seconds = (byte)sec
                              };

            return newPart;
        }

        /// <summary>
        /// Gets the primary chart details for Synastry analysis.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>the first synastry chart's details.</returns>
        [NotNull]
        public async Task<ActionResult> Synastry(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ////var enteredChart = await this.db.EnteredCharts.Include(chart => chart.ChartObjects).FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            var enteredChart = await this.db.EnteredCharts.FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            if (enteredChart == null)
            {
                return this.HttpNotFound();
            }

            return this.View(enteredChart);
        }

        /// <summary>
        /// Gets the primary chart details for Transit analysis.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The base chart's details.</returns>
        [NotNull]
        public async Task<ActionResult> Transits(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ////var enteredChart = await this.db.EnteredCharts.Include(chart => chart.ChartObjects).FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            var enteredChart = await this.db.EnteredCharts.FirstOrDefaultAsync(chart => id == chart.EnteredChartId);
            if (enteredChart == null)
            {
                return this.HttpNotFound();
            }

            return this.View(enteredChart);
        }

        /// <summary>
        /// Updates the angle for entered chart.
        /// </summary>
        /// <param name="chartAngleId">The chart angle identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <returns>
        /// A JSON result.
        /// </returns>
        [CanBeNull]
        public JsonResult UpdateAngleForEnteredChart(
            int chartAngleId, 
            byte degrees, 
            byte signId, 
            byte minutes, 
            byte seconds)
        {
            var updateAngle = this.db.ChartAngles.Find(chartAngleId);

            if (updateAngle == null)
            {
                return this.Json(null, JsonRequestBehavior.DenyGet);
            }

            updateAngle.Degrees = degrees;
            updateAngle.SignId = signId;
            updateAngle.Minutes = minutes;
            updateAngle.Seconds = seconds;

            this.UpdateModel(updateAngle);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Updates the chart object for entered chart.
        /// </summary>
        /// <param name="chartObjectId">The chart object identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="orientationId">The orientation identifier.</param>
        /// <returns>
        /// Success if successful, null otherwise.
        /// </returns>
        [CanBeNull]
        public JsonResult UpdateChartObjectForEnteredChart(
            int chartObjectId, 
            byte degrees, 
            byte signId, 
            byte minutes, 
            byte seconds, 
            byte orientationId)
        {
            var updateObject = this.db.ChartObjects.Find(chartObjectId);

            if (updateObject == null)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            updateObject.Degrees = degrees;
            updateObject.SignId = signId;
            updateObject.Minutes = minutes;
            updateObject.Seconds = seconds;
            updateObject.OrientationId = orientationId;

            this.UpdateModel(updateObject);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the house cusp for entered chart.
        /// </summary>
        /// <param name="chartHouseId">The chart house identifier.</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="signId">The sign identifier.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>
        /// A JSON result.
        /// </returns>
        [CanBeNull]
        public JsonResult UpdateHouseCuspForEnteredChart(
            int chartHouseId, 
            byte degrees, 
            byte signId, 
            byte minutes, 
            byte seconds, 
            byte houseSystemId)
        {
            var updateHouse = this.db.ChartHouses.Find(chartHouseId);

            if (updateHouse == null)
            {
                return this.Json(null, JsonRequestBehavior.DenyGet);
            }

            updateHouse.Degrees = degrees;
            updateHouse.SignId = signId;
            updateHouse.Minutes = minutes;
            updateHouse.Seconds = seconds;

            this.UpdateModel(updateHouse);
            this.db.SaveChanges();

            return this.Json("Success", JsonRequestBehavior.DenyGet);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Releases unmanaged resources and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}