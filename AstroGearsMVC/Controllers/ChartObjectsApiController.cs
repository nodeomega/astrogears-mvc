// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChartObjectsApiController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   The Chart Objects API Controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;

    using AstroGearsMVC.Models;

    using JetBrains.Annotations;

    /// <summary>
    /// The Chart Objects API Controller.
    /// </summary>
    public class ChartObjectsApiController : ApiController
    {
        /// <summary>
        /// The database
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        // GET: api/ChartObjectsApi

        /// <summary>
        /// Gets the chart objects.
        /// </summary>
        /// <param name="chartId">The chart identifier.</param>
        /// <returns>
        /// Returns the chart objects.
        /// </returns>
        [CanBeNull]
        public IQueryable<ChartObject> GetChartObjects(int chartId)
        {
            return this.db.ChartObjects.Where(x => x.EnteredChartID == chartId);
        }

            // GET: api/ChartObjectsApi/5

        /// <summary>
        /// Gets the chart object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Ok or Not Found based on outcome.</returns>
        [ResponseType(typeof(ChartObject))]
        public async Task<IHttpActionResult> GetChartObject(int id)
        {
            var chartObject = await this.db.ChartObjects.FindAsync(id);
            if (chartObject == null)
            {
                return this.NotFound();
            }

            return this.Ok(chartObject);
        }

        // PUT: api/ChartObjectsApi/5

        /// <summary>
        /// Puts the chart object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="chartObject">The chart object.</param>
        /// <returns>Intentionally blank or bad request depending on outcome.</returns>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutChartObject(int id, [NotNull] ChartObject chartObject)
        {
            if (chartObject == null)
            {
                throw new ArgumentNullException("chartObject");
            }

            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (id != chartObject.ChartObjectId)
            {
                return this.BadRequest();
            }

            this.db.Entry(chartObject).State = EntityState.Modified;

            try
            {
                await this.db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.ChartObjectExists(id))
                {
                    return this.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ChartObjectsApi

        /// <summary>
        /// Posts the chart object.
        /// </summary>
        /// <param name="chartObject">The chart object.</param>
        /// <returns></returns>
        [ResponseType(typeof(ChartObject))]
        public async Task<IHttpActionResult> PostChartObject(ChartObject chartObject)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(ModelState);
            }

            this.db.ChartObjects.Add(chartObject);
            await this.db.SaveChangesAsync();

            return this.CreatedAtRoute("DefaultApi", new { id = chartObject.ChartObjectId }, chartObject);
        }

        // DELETE: api/ChartObjectsApi/5
        [ResponseType(typeof(ChartObject))]
        public async Task<IHttpActionResult> DeleteChartObject(int id)
        {
            ChartObject chartObject = await this.db.ChartObjects.FindAsync(id);
            if (chartObject == null)
            {
                return this.NotFound();
            }

            this.db.ChartObjects.Remove(chartObject);
            await this.db.SaveChangesAsync();

            return this.Ok(chartObject);
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources.
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

        /// <summary>
        /// Charts the object exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>True if the chart object exists, false otherwise.</returns>
        private bool ChartObjectExists(int id)
        {
            return this.db.ChartObjects.Count(e => e.ChartObjectId == id) > 0;
        }
    }
}