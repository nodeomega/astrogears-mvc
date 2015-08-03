// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynastryController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the SynastryController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using AstroGearsMVC.Models;

    using JetBrains.Annotations;

    /// <summary>
    /// Defines the SynastryController type.
    /// </summary>
    public class SynastryController : Controller
    {
        #region Fields

        /// <summary>
        ///     The database
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        #endregion

        /// <summary>
        /// Gets the synastry chart listing.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>JSON for a drop down list to select on the Synastry page.</returns>
        public JsonResult GetSynastryChartListing(int chartId)
        {
            var availableCharts =
                this.db.EnteredCharts.Where(
                    x =>
                    x.EnteredChartId != chartId
                    && ((x.ChartTypeId == (byte)EnteredChart.ChartTypes.Natal)
                        || (x.ChartTypeId == (byte)EnteredChart.ChartTypes.Event)
                        || (x.ChartTypeId == (byte)EnteredChart.ChartTypes.DavisonCorrected)
                        || (x.ChartTypeId == (byte)EnteredChart.ChartTypes.DavisonUncorrected)
                        || (x.ChartTypeId == (byte)EnteredChart.ChartTypes.CompositeMidpoint)
                        || (x.ChartTypeId == (byte)EnteredChart.ChartTypes.CompositeReferencePlace)))
                    .Select(x => new { x.EnteredChartId, x.SubjectName, x.ChartType.ChartTypeName });

            return this.Json(availableCharts, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the selected entered chart data.
        /// </summary>
        /// <param name="synastryChartId">The synastry chart identifier.</param>
        /// <returns>The subject location and origin date</returns>
        public JsonResult GetSelectedEnteredChartData(int synastryChartId)
        {
            var thisChart = this.db.EnteredCharts.Find(synastryChartId);

            return this.Json(
                new { thisChart.EnteredChartId, thisChart.SubjectLocation, thisChart.OriginDateTimeString },
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the conjunct chart objects.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="firstChartId">The chart identifier.</param>
        /// <param name="secondChartId">The second chart identifier.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="midpoints">if set to <c>true</c> [midpoints].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>A JSON array of chart objects and their aspects to the specified chart object.</returns>
        [CanBeNull]
        public JsonResult GetSynastryAspectChartObjects(
            int id,
            int firstChartId,
            int secondChartId,
            bool draconic,
            bool arabic,
            bool asteroids,
            bool stars,
            bool midpoints,
            byte houseSystemId)
        {
            if (id <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var chartCon = new EnteredChartsController();

            var firstChartObject = this.db.ChartObjects.Find(id);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == secondChartId)
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
                    .Union(chartCon.GetAngleChartObjects(secondChartId))
                    .Union(chartCon.GetArabicPartChartObjects(secondChartId, houseSystemId, arabic))
                    .Union(chartCon.GetDraconicChartObjects(secondChartId, draconic, arabic, asteroids, midpoints, houseSystemId))
                    .Union(chartCon.GetMidpointChartObjects(secondChartId, midpoints))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.DisplayOrder).ToList();

            var aspectObjectLists = chartCon.GetAspectObjectLists(thisChart, firstChartObject, secondChartId, houseSystemId);

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
        /// <param name="firstChartId">The chart identifier.</param>
        /// <param name="secondChartId">The second chart identifier.</param>
        /// <param name="angleName">Name of the angle.</param>
        /// <param name="angleCoordinates">The angle coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="midpoints">if set to <c>true</c> [midpoints].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>a JSON object containing all of the objects that aspect the given Arabic Part.</returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetSynastryAspectChartObjectsForAngle(
            int firstChartId,
            int secondChartId,
            [CanBeNull] string angleName,
            [CanBeNull] string angleCoordinates,
            bool draconic,
            bool arabic,
            bool asteroids,
            bool stars,
            bool midpoints,
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(angleName) || string.IsNullOrEmpty(angleCoordinates) || firstChartId <= 0 || secondChartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var chartCon = new EnteredChartsController();

            var angles = chartCon.GetAngleChartObjects(firstChartId);

            var comparisonAngles = chartCon.GetAngleChartObjects(secondChartId);

            if (angles.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = angles.FirstOrDefault(ap => ap.CelestialObject.CelestialObjectName == angleName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == secondChartId)
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
                    .Union(comparisonAngles)
                    .Union(chartCon.GetArabicPartChartObjects(secondChartId, houseSystemId, arabic))
                    .Union(chartCon.GetDraconicChartObjects(secondChartId, draconic, arabic, asteroids, midpoints, houseSystemId))
                    .Union(chartCon.GetMidpointChartObjects(secondChartId, midpoints))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.DisplayOrder).ToList();

            var aspectObjectLists = chartCon.GetAspectObjectLists(thisChart, thisObject, secondChartId, houseSystemId);

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
        /// <param name="firstChartId">The chart identifier.</param>
        /// <param name="secondChartId">The second chart identifier.</param>
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
        public JsonResult GetSynastryAspectChartObjectsForArabicPart(
            int firstChartId,
            int secondChartId,
            [CanBeNull] string arabicPartName,
            [CanBeNull] string arabicPartCoordinates,
            bool draconic,
            bool arabic,
            bool asteroids,
            bool stars,
            bool midpoints,
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(arabicPartName) || string.IsNullOrEmpty(arabicPartCoordinates) || firstChartId <= 0 || secondChartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var chartCon = new EnteredChartsController();

            var arabicParts = chartCon.GetArabicPartChartObjects(firstChartId, houseSystemId, arabic);

            if (arabicParts.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var thisObject = arabicParts.FirstOrDefault(ap => ap.CelestialObject.CelestialObjectName == arabicPartName);

            arabicParts = null;

            var secondArabicParts = chartCon.GetArabicPartChartObjects(secondChartId, houseSystemId, arabic);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == secondChartId)
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
                    .Union(chartCon.GetAngleChartObjects(secondChartId))
                    .Union(secondArabicParts)
                    .Union(chartCon.GetDraconicChartObjects(secondChartId, draconic, arabic, asteroids, midpoints, houseSystemId))
                    .Union(chartCon.GetMidpointChartObjects(secondChartId, midpoints))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.DisplayOrder).ToList();

            var aspectObjectLists = chartCon.GetAspectObjectLists(thisChart, thisObject, secondChartId, houseSystemId);

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
        /// Gets the aspect chart objects for draconic object.
        /// </summary>
        /// <param name="firstChartId">The chart identifier.</param>
        /// <param name="secondChartId">The second chart identifier.</param>
        /// <param name="draconicName">Name of the draconic.</param>
        /// <param name="draconicCoordinates">The draconic coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="midpoints">if set to <c>true</c> [midpoints].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>a JSON object containing all of the objects that aspect the given Draconic Object.</returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetSynastryAspectChartObjectsForDraconicObject(
            int firstChartId,
            int secondChartId,
            [CanBeNull] string draconicName,
            [CanBeNull] string draconicCoordinates,
            bool draconic,
            bool arabic,
            bool asteroids,
            bool stars,
            bool midpoints,
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(draconicName) || string.IsNullOrEmpty(draconicCoordinates) || firstChartId <= 0 || secondChartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var chartCon = new EnteredChartsController();

            var draconicObjects = chartCon.GetDraconicChartObjects(
                firstChartId,
                draconic,
                arabic,
                asteroids,
                midpoints,
                houseSystemId);

            if (draconicObjects.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var secondDraconicObjects = chartCon.GetDraconicChartObjects(
                secondChartId,
                draconic,
                arabic,
                asteroids,
                midpoints,
                houseSystemId);

            var thisObject = draconicObjects.FirstOrDefault(
                ap => ap.CelestialObject.CelestialObjectName == draconicName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == secondChartId)
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
                    .Union(chartCon.GetAngleChartObjects(secondChartId))
                    .Union(chartCon.GetArabicPartChartObjects(secondChartId, houseSystemId, arabic))
                    .Union(secondDraconicObjects)
                    .Union(chartCon.GetMidpointChartObjects(secondChartId, midpoints))
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.DisplayOrder).ToList();

            var aspectObjectLists = chartCon.GetAspectObjectLists(thisChart, thisObject, secondChartId, houseSystemId);

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
        /// Gets the aspect chart objects for midpoint object.
        /// </summary>
        /// <param name="firstChartId">The chart identifier.</param>
        /// <param name="secondChartId">The second chart identifier.</param>
        /// <param name="midpointName">Name of the draconic.</param>
        /// <param name="midpointCoordinates">The draconic coordinates.</param>
        /// <param name="draconic">if set to <c>true</c> [draconic].</param>
        /// <param name="arabic">if set to <c>true</c> [arabic].</param>
        /// <param name="asteroids">if set to <c>true</c> [asteroids].</param>
        /// <param name="stars">if set to <c>true</c> [stars].</param>
        /// <param name="midpoints">if set to <c>true</c> [midpoints].</param>
        /// <param name="houseSystemId">The house system identifier.</param>
        /// <returns>a JSON object containing all of the objects that aspect the given Draconic Object.</returns>
        [CanBeNull]
        [ValidateInput(false)]
        public JsonResult GetSynastryAspectChartObjectsForMidpoint(
            int firstChartId,
            int secondChartId,
            [CanBeNull] string midpointName,
            [CanBeNull] string midpointCoordinates,
            bool draconic,
            bool arabic,
            bool asteroids,
            bool stars,
            bool midpoints,
            byte houseSystemId)
        {
            if (string.IsNullOrEmpty(midpointName) || string.IsNullOrEmpty(midpointCoordinates) || firstChartId <= 0 || secondChartId <= 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var chartCon = new EnteredChartsController();

            var midpointObjects = chartCon.GetMidpointChartObjects(firstChartId, midpoints);

            if (midpointObjects.Count == 0)
            {
                return this.Json(null, JsonRequestBehavior.AllowGet);
            }

            var secondMidpointObjects = chartCon.GetMidpointChartObjects(secondChartId, midpoints);

            var thisObject = midpointObjects.FirstOrDefault(
                ap => ap.CelestialObject.CelestialObjectName == midpointName);

            var thisChart =
                this.db.ChartObjects.Where(x => x.EnteredChartID == secondChartId)
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
                    .Union(chartCon.GetAngleChartObjects(secondChartId))
                    .Union(chartCon.GetArabicPartChartObjects(secondChartId, houseSystemId, arabic))
                    .Union(chartCon.GetDraconicChartObjects(secondChartId, draconic, arabic, asteroids, midpoints, houseSystemId))
                    .Union(secondMidpointObjects)
                    .OrderBy(x => x.CalculatedCoordinate);

            var aspectList = this.db.Aspects.OrderBy(a => a.DisplayOrder).ToList();

            var aspectObjectLists = chartCon.GetAspectObjectLists(thisChart, thisObject, secondChartId, houseSystemId);

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
    }
}