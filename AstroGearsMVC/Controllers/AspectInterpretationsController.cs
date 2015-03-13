// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AspectInterpretationsController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the AspectInterpretationsController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using AstroGearsMVC.Models;

    using JetBrains.Annotations;

    /// <summary>
    /// The AspectInterpretations Controller.
    /// </summary>
    public class AspectInterpretationsController : Controller
    {
        /// <summary>
        /// The database
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>The list of Aspect Interpretations.</returns>
        [CanBeNull]
        public async Task<ActionResult> Index()
        {
            var aspectInterpretations =
                this.db.AspectInterpretations.Include(a => a.AspectInterpretationType)
                    .Include(a => a.Aspect)
                    .Include(a => a.CelestialObject)
                    .Include(a => a.CelestialObject1)
                    .Include(a => a.HouseAngle);
            return this.View(await aspectInterpretations.ToListAsync());
        }

        /// <summary>
        /// Gets the aspect interpretation listing.
        /// </summary>
        /// <param name="pageNum">The page number.</param>
        /// <param name="entriesPerPage">The entries per page.</param>
        /// <returns>
        /// The aspect interpretation listing.
        /// </returns>
        [CanBeNull]
        public JsonResult GetAspectInterpretationListing(int pageNum, int entriesPerPage)
        {
            var aspectInterpretations =
                this.db.AspectInterpretations;

            var page =
                aspectInterpretations.OrderBy(x => x.CelestialObject.CelestialObjectTypeId)
                    .ThenBy(x => x.AspectInterpretationTypeId1)
                    .ThenBy(x => x.CelestialObject.CelestialObjectId)
                    .ThenBy(x => x.AngleId1)
                    .ThenBy(x => x.CelestialObject1.CelestialObjectTypeId)
                    .ThenBy(x => x.AspectInterpretationTypeId2)
                    .ThenBy(x => x.CelestialObject1.CelestialObjectId)
                    .ThenBy(x => x.AngleId2)
                    .Select(
                        x =>
                        new
                            {
                                x.AspectInterpretationId,
                                FirstObject =
                            x.AspectInterpretationType.AspectInterpretationTypeName + " "
                            + (x.CelestialObjectId1.HasValue
                                   ? x.CelestialObject.CelestialObjectName
                                   : (x.AngleId1.HasValue ? x.HouseAngle.AngleName : "N/A")),
                                SecondObject =
                            x.AspectInterpretationType1.AspectInterpretationTypeName + " "
                            + (x.CelestialObjectId2.HasValue
                                   ? x.CelestialObject1.CelestialObjectName
                                   : (x.AngleId2.HasValue ? x.HouseAngle1.AngleName : "N/A")),
                                x.Aspect.AspectName,
                                x.Interpretation,
                                x.CitationUrl
                            })
                    .Skip((pageNum - 1) * entriesPerPage)
                    .Take(entriesPerPage)
                    .GroupBy(x => new { Total = aspectInterpretations.Count() })
                    .FirstOrDefault();

            var total = (page != null) ? page.Key.Total : 0;

            var listing = page.Select(x => x);

            var returnObj = new object[] { listing, Math.Ceiling((decimal)total / entriesPerPage) };

            ////return this.Json(aspectInterpretations, JsonRequestBehavior.AllowGet);

            return this.Json(returnObj, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// Gets the aspect interpretation listing for a single aspect type from a natal charts detail.
        /// </summary>
        /// <param name="celestialObjectId1">The celestial object id1.</param>
        /// <param name="angleId1">The angle id1.</param>
        /// <param name="aspectId">The aspect identifier.</param>
        /// <param name="celestialObjectId2">The celestial object id2.</param>
        /// <param name="angleId2">The angle id2.</param>
        /// <returns>
        /// The aspect interpretation listing.
        /// </returns>
        [CanBeNull]
        public JsonResult GetSingleChartDetailAspectInterpretationRequest(
            int? celestialObjectId1,
            byte? angleId1,
            byte aspectId,
            int? celestialObjectId2,
            byte? angleId2)
        {
            var aspectInterpretations =
                this.db.AspectInterpretations.Where(
                    x =>
                    x.CelestialObjectId1 == celestialObjectId1 && x.AngleId1 == angleId1 && x.AspectId == aspectId
                    && x.CelestialObjectId2 == celestialObjectId2 && x.AngleId2 == angleId2
                    && x.AspectInterpretationTypeId1 == 1 && x.AspectInterpretationTypeId2 == 1)
                    .Union(
                        this.db.AspectInterpretations.Where(
                            x =>
                            x.CelestialObjectId1 == celestialObjectId2 && x.AngleId1 == angleId2
                            && x.AspectId == aspectId && x.CelestialObjectId2 == celestialObjectId1
                            && x.AngleId2 == angleId1 && x.AspectInterpretationTypeId1 == 1
                            && x.AspectInterpretationTypeId2 == 1))
                    .Select(
                        x =>
                        new
                            {
                                x.AspectInterpretationId,
                                FirstObject =
                            x.AspectInterpretationType.AspectInterpretationTypeName + " "
                            + (x.CelestialObjectId1.HasValue
                                   ? x.CelestialObject.CelestialObjectName
                                   : (x.AngleId1.HasValue ? x.HouseAngle.AngleName : "N/A")),
                                SecondObject =
                            x.AspectInterpretationType1.AspectInterpretationTypeName + " "
                            + (x.CelestialObjectId2.HasValue
                                   ? x.CelestialObject1.CelestialObjectName
                                   : (x.AngleId2.HasValue ? x.HouseAngle1.AngleName : "N/A")),
                                x.Aspect.AspectName,
                                x.Interpretation,
                                x.CitationUrl
                            })
                    .ToList().Distinct();

            return this.Json(aspectInterpretations, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the synastry chart detail aspect interpretation request.
        /// </summary>
        /// <param name="celestialObjectId1">The celestial object id1.</param>
        /// <param name="angleId1">The angle id1.</param>
        /// <param name="aspectId">The aspect identifier.</param>
        /// <param name="celestialObjectId2">The celestial object id2.</param>
        /// <param name="angleId2">The angle id2.</param>
        /// <returns>The synastry aspect interpretation.</returns>
        [CanBeNull]
        public JsonResult GetSynastryChartDetailAspectInterpretationRequest(
            int? celestialObjectId1,
            byte? angleId1,
            byte aspectId,
            int? celestialObjectId2,
            byte? angleId2)
        {
            var aspectInterpretations =
                this.db.AspectInterpretations.Where(
                    x =>
                    x.CelestialObjectId1 == celestialObjectId1 && x.AngleId1 == angleId1 && x.AspectId == aspectId
                    && x.CelestialObjectId2 == celestialObjectId2 && x.AngleId2 == angleId2
                    && x.AspectInterpretationTypeId1 == 4 && x.AspectInterpretationTypeId2 == 4)
                    .Union(
                        this.db.AspectInterpretations.Where(
                            x =>
                            x.CelestialObjectId1 == celestialObjectId2 && x.AngleId1 == angleId2
                            && x.AspectId == aspectId && x.CelestialObjectId2 == celestialObjectId1
                            && x.AngleId2 == angleId1 && x.AspectInterpretationTypeId1 == 4
                            && x.AspectInterpretationTypeId2 == 4))
                    .Select(
                        x =>
                        new
                        {
                            x.AspectInterpretationId,
                            FirstObject =
                        x.AspectInterpretationType.AspectInterpretationTypeName + " "
                        + (x.CelestialObjectId1.HasValue
                               ? x.CelestialObject.CelestialObjectName
                               : (x.AngleId1.HasValue ? x.HouseAngle.AngleName : "N/A")),
                            SecondObject =
                        x.AspectInterpretationType1.AspectInterpretationTypeName + " "
                        + (x.CelestialObjectId2.HasValue
                               ? x.CelestialObject1.CelestialObjectName
                               : (x.AngleId2.HasValue ? x.HouseAngle1.AngleName : "N/A")),
                            x.Aspect.AspectName,
                            x.Interpretation,
                            x.CitationUrl
                        })
                    .ToList().Distinct();

            return this.Json(aspectInterpretations, JsonRequestBehavior.AllowGet);
        }

        private void GetChartInterpretationTypeForTransits(byte chartTypeId, out byte chartInterpretationTypeId)
        {
            switch (chartTypeId)
            {
                case (byte)EnteredChart.ChartTypes.Natal:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Natal;
                    break;
                case (byte)EnteredChart.ChartTypes.Transit:
                case (byte)EnteredChart.ChartTypes.Event:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Transit;
                    break;
                case (byte)EnteredChart.ChartTypes.Progressed:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Progression;
                    break;
                case (byte)EnteredChart.ChartTypes.Heliocentric:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Heliocentric;
                    break;
                case (byte)EnteredChart.ChartTypes.CompositeMidpoint:
                case (byte)EnteredChart.ChartTypes.CompositeReferencePlace:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Composite;
                    break;
                case (byte)EnteredChart.ChartTypes.DavisonCorrected:
                case (byte)EnteredChart.ChartTypes.DavisonUncorrected:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.Davison;
                    break;
                case (byte)EnteredChart.ChartTypes.SolarReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.SolarReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.LunarReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.LunarReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.MercuryReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.MercuryReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.VenusReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.VenusReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.MarsReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.MarsReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.JupiterReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.JupiterReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.SaturnReturn:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.SaturnReturn;
                    break;
                case (byte)EnteredChart.ChartTypes.SolarEclipse:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.SolarEclipse;
                    break;
                case (byte)EnteredChart.ChartTypes.LunarEclipse:
                    chartInterpretationTypeId = (byte)AspectInterpretation.InterpretationTypes.LunarEclipse;
                    break;
                default:
                    chartInterpretationTypeId = 0;
                    break;
            }
        }

        /// <summary>
        /// Gets the transit chart detail aspect interpretation request.
        /// </summary>
        /// <param name="chartTypeId1">The chart type id1.</param>
        /// <param name="celestialObjectId1">The celestial object id1.</param>
        /// <param name="angleId1">The angle id1.</param>
        /// <param name="aspectId">The aspect identifier.</param>
        /// <param name="chartTypeId2">The chart type id2.</param>
        /// <param name="celestialObjectId2">The celestial object id2.</param>
        /// <param name="angleId2">The angle id2.</param>
        /// <returns>The Transit aspect interpretation.</returns>
        [CanBeNull]
        public JsonResult GetTransitChartDetailAspectInterpretationRequest(
            byte chartTypeId1,
            int? celestialObjectId1,
            byte? angleId1,
            byte aspectId,
            byte chartTypeId2,
            int? celestialObjectId2,
            byte? angleId2)
        {
            byte chartInterpretationTypeId1, chartInterpretationTypeId2;

            this.GetChartInterpretationTypeForTransits(chartTypeId1, out chartInterpretationTypeId1);
            this.GetChartInterpretationTypeForTransits(chartTypeId2, out chartInterpretationTypeId2);

            var aspectInterpretations =
                this.db.AspectInterpretations.Where(
                    x =>
                    x.CelestialObjectId1 == celestialObjectId1 && x.AngleId1 == angleId1 && x.AspectId == aspectId
                    && x.CelestialObjectId2 == celestialObjectId2 && x.AngleId2 == angleId2
                    && x.AspectInterpretationTypeId1 == chartInterpretationTypeId1 && x.AspectInterpretationTypeId2 == chartInterpretationTypeId2)
                    .Union(
                        this.db.AspectInterpretations.Where(
                            x =>
                            x.CelestialObjectId1 == celestialObjectId2 && x.AngleId1 == angleId2
                            && x.AspectId == aspectId && x.CelestialObjectId2 == celestialObjectId1
                            && x.AngleId2 == angleId1 && x.AspectInterpretationTypeId1 == chartInterpretationTypeId2
                            && x.AspectInterpretationTypeId2 == chartInterpretationTypeId1))
                    .Select(
                        x =>
                        new
                        {
                            x.AspectInterpretationId,
                            FirstObject =
                        x.AspectInterpretationType.AspectInterpretationTypeName + " "
                        + (x.CelestialObjectId1.HasValue
                               ? x.CelestialObject.CelestialObjectName
                               : (x.AngleId1.HasValue ? x.HouseAngle.AngleName : "N/A")),
                            SecondObject =
                        x.AspectInterpretationType1.AspectInterpretationTypeName + " "
                        + (x.CelestialObjectId2.HasValue
                               ? x.CelestialObject1.CelestialObjectName
                               : (x.AngleId2.HasValue ? x.HouseAngle1.AngleName : "N/A")),
                            x.Aspect.AspectName,
                            x.Interpretation,
                            x.CitationUrl
                        })
                    .ToList().Distinct();

            return this.Json(aspectInterpretations, JsonRequestBehavior.AllowGet);
        }

        // GET: AspectInterpretations/Details/5
        
        /// <summary>
        /// Details the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The details of a given Aspect Interpretation.</returns>
        [NotNull]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var aspectInterpretation = await this.db.AspectInterpretations.FindAsync(id);
            if (aspectInterpretation == null)
            {
                return this.HttpNotFound();
            }

            return this.View(aspectInterpretation);
        }

        // GET: AspectInterpretations/Create

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>The Create view.</returns>
        [NotNull]
        public ActionResult Create()
        {
            ////ViewBag.AspectInterpretationTypeId = new SelectList(this.db.AspectInterpretationTypes, "AspectInterpretationTypeId", "AspectInterpretationTypeName");
            ViewBag.AspectId = new SelectList(this.db.Aspects, "AspectId", "AspectName");
            ////ViewBag.CelestialObjectId1 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName");
            ////ViewBag.CelestialObjectId2 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName");
            ViewBag.AngleId = new SelectList(this.db.HouseAngles, "AngleId", "AngleName");
            return this.View();
        }

        // POST: AspectInterpretations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /////// <summary>
        /////// Creates the specified aspect interpretation.
        /////// </summary>
        /////// <param name="aspectInterpretation">The aspect interpretation.</param>
        /////// <returns>The view.</returns>
        /////// <exception cref="System.ArgumentNullException">aspectInterpretation is null</exception>
        ////[NotNull, HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public async Task<ActionResult> Create([NotNull, Bind(Include = "AspectInterpretationId,CelestialObjectId1,AspectId,CelestialObjectId2,AngleId,AspectInterpretationTypeId1,AspectInterpretationTypeId2,Interpretation,CitationUrl")] AspectInterpretation aspectInterpretation)
        ////{
        ////    if (aspectInterpretation == null)
        ////    {
        ////        throw new ArgumentNullException("aspectInterpretation");
        ////    }

        ////    if (ModelState.IsValid)
        ////    {
        ////        this.db.AspectInterpretations.Add(aspectInterpretation);
        ////        await this.db.SaveChangesAsync();
        ////        return this.RedirectToAction("Index");
        ////    }

        ////    ViewBag.AspectInterpretationTypeId1 = new SelectList(this.db.AspectInterpretationTypes, "AspectInterpretationTypeId", "AspectInterpretationType", aspectInterpretation.AspectInterpretationTypeId1);
        ////    ViewBag.AspectInterpretationTypeId2 = new SelectList(this.db.AspectInterpretationTypes, "AspectInterpretationTypeId", "AspectInterpretationType", aspectInterpretation.AspectInterpretationTypeId2);
        ////    ViewBag.AspectId = new SelectList(this.db.Aspects, "AspectId", "AspectName", aspectInterpretation.AspectId);
        ////    ////ViewBag.CelestialObjectId1 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId1);
        ////    ////ViewBag.CelestialObjectId2 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId2);
        ////    ViewBag.AngleId = new SelectList(this.db.HouseAngles, "AngleId", "AngleName", aspectInterpretation.AngleId2);
        ////    return this.View(aspectInterpretation);
        ////}

        /// <summary>
        /// Gets the aspect interpretation types.
        /// </summary>
        /// <returns>A JSON array of the aspect interpretation types and their ids.</returns>
        [CanBeNull]
        public JsonResult GetAspectInterpretationTypes()
        {
            var aspectTypes =
                this.db.AspectInterpretationTypes.Select(
                    x => new { x.AspectInterpretationTypeId, x.AspectInterpretationTypeName });

            return this.Json(aspectTypes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect types.
        /// </summary>
        /// <returns>A JSON array of the aspect types and their ids.</returns>
        [CanBeNull]
        public JsonResult GetAspectsList()
        {
            var aspectTypes = this.db.Aspects.Select(x => new { x.AspectId, x.AspectName });

            return this.Json(aspectTypes, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the angles list.
        /// </summary>
        /// <returns>A JSON array of the angles and their ids.</returns>
        [CanBeNull]
        public JsonResult GetAnglesList()
        {
            var angles = this.db.HouseAngles.Select(x => new { x.AngleId, x.AngleName });

            return this.Json(angles, JsonRequestBehavior.AllowGet);
        }


        // GET: AspectInterpretations/Edit/5
        
        /// <summary>
        /// Edits the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The Task result.</returns>
        [NotNull]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var aspectInterpretation = await this.db.AspectInterpretations.FindAsync(id);
            if (aspectInterpretation == null)
            {
                return this.HttpNotFound();
            }

            ////ViewBag.AspectInterpretationTypeId1 = new SelectList(this.db.AspectInterpretationTypes, "AspectInterpretationTypeId1", "AspectInterpretationTypeName", aspectInterpretation.AspectInterpretationTypeId1);
            ////ViewBag.AspectId = new SelectList(this.db.Aspects, "AspectId", "AspectName", aspectInterpretation.AspectId);
            ////ViewBag.CelestialObjectId1 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId1);
            ////ViewBag.CelestialObjectId2 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId2);
            ////ViewBag.AngleId = new SelectList(this.db.HouseAngles, "AngleId", "AngleName", aspectInterpretation.AngleId2);
            return this.View(aspectInterpretation);
        }

        /// <summary>
        /// Gets the aspect interpretation for edit.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The interpreted ID.</returns>
        [NotNull]
        public JsonResult GetAspectInterpretationForEdit(int? id)
        {
            if (id == null)
            {
                return this.Json("FAILED", JsonRequestBehavior.AllowGet);
            }

            var thisEdit =
                this.db.AspectInterpretations.Where(x => x.AspectInterpretationId == id.Value)
                    .Select(
                        x =>
                        new
                            {
                                x.AspectInterpretationId,
                                x.AspectInterpretationTypeId1,
                                x.CelestialObjectId1,
                                CelestialObjectName1 = x.CelestialObject.CelestialObjectName,
                                x.AngleId1,
                                x.AspectInterpretationTypeId2,
                                x.CelestialObjectId2,
                                CelestialObjectName2 = x.CelestialObject1.CelestialObjectName,
                                x.AngleId2,
                                x.AspectId,
                                x.Interpretation,
                                x.CitationUrl
                            });

            return this.Json(thisEdit, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the aspect interpretation for delete.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The aspect interpretation info for deletion.</returns>
        [CanBeNull]
        public JsonResult GetAspectInterpretationForDelete(int? id)
        {
            if (id == null)
            {
                return this.Json("Not Found for Delete", JsonRequestBehavior.AllowGet);
            }

            var thisDelete =
                this.db.AspectInterpretations.Where(x => x.AspectInterpretationId == id.Value)
                    .Select(
                        x =>
                        new
                            {
                                x.AspectInterpretationId,
                                FirstObject = x.AspectInterpretationType.AspectInterpretationTypeName + " " + (x.CelestialObjectId1.HasValue ? x.CelestialObject.CelestialObjectName : (x.AngleId1.HasValue ? x.HouseAngle.AngleName : "N/A")),
                                SecondObject = x.AspectInterpretationType1.AspectInterpretationTypeName + " " + (x.CelestialObjectId2.HasValue ? x.CelestialObject1.CelestialObjectName : (x.AngleId2.HasValue ? x.HouseAngle1.AngleName : "N/A")),
                                x.Aspect.AspectName,
                                x.Interpretation,
                                x.CitationUrl
                            });

            return this.Json(thisDelete, JsonRequestBehavior.AllowGet);
        }

        // POST: AspectInterpretations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        /////// <summary>
        /////// Edits the specified aspect interpretation.
        /////// </summary>
        /////// <param name="aspectInterpretation">The aspect interpretation.</param>
        /////// <returns>The result.</returns>
        /////// <exception cref="System.ArgumentNullException">aspectInterpretation is null.</exception>
        ////[NotNull, HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public async Task<ActionResult> Edit([NotNull] [Bind(Include = "AspectInterpretationId,CelestialObjectId1,AspectId,CelestialObjectId2,AngleId,AspectInterpretationTypeId1,AspectInterpretationTypeId2,Interpretation,CitationUrl")] AspectInterpretation aspectInterpretation)
        ////{
        ////    if (aspectInterpretation == null)
        ////    {
        ////        throw new ArgumentNullException("aspectInterpretation");
        ////    }

        ////    if (ModelState.IsValid)
        ////    {
        ////        this.db.Entry(aspectInterpretation).State = EntityState.Modified;
        ////        await this.db.SaveChangesAsync();
        ////        return this.RedirectToAction("Index");
        ////    }

        ////    ViewBag.AspectInterpretationTypeId1 = new SelectList(this.db.AspectInterpretationTypes, "AspectInterpretationTypeId", "AspectInterpretationType1", aspectInterpretation.AspectInterpretationTypeId1);
        ////    ViewBag.AspectId = new SelectList(this.db.Aspects, "AspectId", "AspectName", aspectInterpretation.AspectId);
        ////    ViewBag.CelestialObjectId1 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId1);
        ////    ViewBag.CelestialObjectId2 = new SelectList(this.db.CelestialObjects, "CelestialObjectId", "CelestialObjectName", aspectInterpretation.CelestialObjectId2);
        ////    ViewBag.AngleId = new SelectList(this.db.HouseAngles, "AngleId", "AngleName", aspectInterpretation.AngleId);
        ////    return this.View(aspectInterpretation);
        ////}

        // GET: AspectInterpretations/Delete/5

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The result.</returns>
        [NotNull]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var aspectInterpretation = await this.db.AspectInterpretations.FindAsync(id);
            if (aspectInterpretation == null)
            {
                return this.HttpNotFound();
            }

            return this.View(aspectInterpretation);
        }

        // POST: AspectInterpretations/Delete/5

        /// <summary>
        /// Deletes the confirmed.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The result</returns>
        [NotNull, HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var aspectInterpretation = await this.db.AspectInterpretations.FindAsync(id);
            this.db.AspectInterpretations.Remove(aspectInterpretation);
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// Gets the full autocomplete data for asteroids and fixed stars.
        /// </summary>
        /// <param name="enteredTerm">The entered term.</param>
        /// <returns>
        /// The full list of Asteroids and Fixed Stars to autocomplete.
        /// </returns>
        [CanBeNull]
        public JsonResult GetFullAutoComplete([NotNull] string enteredTerm)
        {
            if (enteredTerm == null)
            {
                throw new ArgumentNullException("enteredTerm");
            }

            return
                this.Json(
                    this.db.AutoCompleteForAspectInterpretationEntry(enteredTerm)
                        .OrderBy(x => x.CelestialObjectTypeId)
                        .Select(x => new { x.CelestialObjectName, x.CelestialObjectId, x.CelestialObjectTypeId }),
                    JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Creates the new aspect interpretation.
        /// </summary>
        /// <param name="aspectInterpretationTypeId1">The aspect interpretation type id1.</param>
        /// <param name="celestialObjectId1">The celestial object id1.</param>
        /// <param name="angleId1">The angle id1.</param>
        /// <param name="aspectId">The aspect identifier.</param>
        /// <param name="aspectInterpretationTypeId2">The aspect interpretation type id2.</param>
        /// <param name="celestialObjectId2">The celestial object id2.</param>
        /// <param name="angleId2">The angle id2.</param>
        /// <param name="interpretation">The interpretation.</param>
        /// <param name="citationUrl">The citation URL.</param>
        /// <returns>
        /// Success if this works..
        /// </returns>
        /// <exception cref="System.ArgumentNullException">interpretation is null</exception>
        [CanBeNull]
        public JsonResult CreateNewAspectInterpretation(
            byte aspectInterpretationTypeId1,
            int? celestialObjectId1,
            byte? angleId1,
            byte aspectId,
            byte aspectInterpretationTypeId2, 
            int? celestialObjectId2,
            byte? angleId2, 
            [NotNull] string interpretation, 
            [CanBeNull] string citationUrl)
        {
            if (interpretation == null)
            {
                throw new ArgumentNullException("interpretation");
            }

            try
            {
                var newAspectInterpretation = new AspectInterpretation
                                                  {
                                                      AspectInterpretationTypeId1 =
                                                          aspectInterpretationTypeId1,
                                                      AspectInterpretationTypeId2 =
                                                          aspectInterpretationTypeId2,
                                                      CelestialObjectId1 = celestialObjectId1,
                                                      CelestialObjectId2 = celestialObjectId2,
                                                      AngleId1 = angleId1,
                                                      AngleId2 = angleId2,
                                                      AspectId = aspectId,
                                                      Interpretation = interpretation,
                                                      CitationUrl = citationUrl
                                                  };

                this.db.AspectInterpretations.Add(newAspectInterpretation);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Updates the aspect interpretation.
        /// </summary>
        /// <param name="aspectInterpretationId">The aspect interpretation identifier.</param>
        /// <param name="aspectInterpretationTypeId1">The aspect interpretation type id1.</param>
        /// <param name="celestialObjectId1">The celestial object id1.</param>
        /// <param name="angleId1">The angle id1.</param>
        /// <param name="aspectId">The aspect identifier.</param>
        /// <param name="aspectInterpretationTypeId2">The aspect interpretation type id2.</param>
        /// <param name="celestialObjectId2">The celestial object id2.</param>
        /// <param name="angleId2">The angle id2.</param>
        /// <param name="interpretation">The interpretation.</param>
        /// <param name="citationUrl">The citation URL.</param>
        /// <returns>JSON result stating pass or fail.</returns>
        /// <exception cref="System.ArgumentNullException">interpretation is null</exception>
        [CanBeNull]
        public JsonResult UpdateAspectInterpretation(
            int aspectInterpretationId,
            byte aspectInterpretationTypeId1,
            int? celestialObjectId1,
            byte? angleId1,
            byte aspectId,
            byte aspectInterpretationTypeId2,
            int? celestialObjectId2,
            byte? angleId2,
            [NotNull] string interpretation,
            [CanBeNull] string citationUrl)
        {
            if (interpretation == null)
            {
                throw new ArgumentNullException("interpretation");
            }

            try
            {
                var aspectInterpretation = this.db.AspectInterpretations.Find(aspectInterpretationId);

                if (aspectInterpretation == null)
                {
                    return this.Json("Failed: Cannot find specified Aspect Interpretation", JsonRequestBehavior.DenyGet);
                }

                aspectInterpretation.AspectInterpretationTypeId1 = aspectInterpretationTypeId1;
                aspectInterpretation.AspectInterpretationTypeId2 = aspectInterpretationTypeId2;
                aspectInterpretation.CelestialObjectId1 = celestialObjectId1;
                aspectInterpretation.CelestialObjectId2 = celestialObjectId2;
                aspectInterpretation.AngleId1 = angleId1;
                aspectInterpretation.AngleId2 = angleId2;
                aspectInterpretation.AspectId = aspectId;
                aspectInterpretation.Interpretation = interpretation ?? "N/A";
                aspectInterpretation.CitationUrl = citationUrl;
                
                this.UpdateModel(aspectInterpretation);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Confirms the delete of aspect interpretation.
        /// </summary>
        /// <param name="aspectInterpretationId">The aspect interpretation identifier.</param>
        /// <returns>a return message.</returns>
        [CanBeNull]
        public JsonResult ConfirmDeleteOfAspectInterpretation(int aspectInterpretationId)
        {
            try
            {
                var deleteAspectInterpretation = this.db.AspectInterpretations.Find(aspectInterpretationId);

                if (deleteAspectInterpretation == null)
                {
                    return this.Json("Failed: Cannot find specified Aspect Interpretation", JsonRequestBehavior.DenyGet);
                }

                this.db.AspectInterpretations.Remove(deleteAspectInterpretation);
                this.db.SaveChanges();

                return this.Json("Success", JsonRequestBehavior.DenyGet);
            }
            catch (Exception ex)
            {
                return this.Json("Failed: " + ex.Message, JsonRequestBehavior.DenyGet);
            }
        }

        /// <summary>
        /// Releases unmanaged resources and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
