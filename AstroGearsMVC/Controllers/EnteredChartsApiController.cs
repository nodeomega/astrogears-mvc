// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnteredChartsApiController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// <summary>
//   Defines the EnteredChartsApiController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
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
    /// Entered Charts Controller for API calls.
    /// </summary>
    public class EnteredChartsApiController : ApiController
    {
        /// <summary>
        /// The database
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        // GET: api/EnteredChartsApi

        /// <summary>
        /// Deletes the entered chart.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The Task.</returns>
        [NotNull][ResponseType(typeof(EnteredChart))]
        public async Task<IHttpActionResult> DeleteEnteredChart(int id)
        {
            var enteredChart = await this.db.EnteredCharts.FindAsync(id);
            if (enteredChart == null)
            {
                return this.NotFound();
            }

            this.db.EnteredCharts.Remove(enteredChart);
            await this.db.SaveChangesAsync();

            return this.Ok(enteredChart);
        }

        [ResponseType(typeof(EnteredChart))]
        public async Task<IHttpActionResult> GetEnteredChart(int id)
        {
            EnteredChart enteredChart = await this.db.EnteredCharts.FindAsync(id);
            if (enteredChart == null)
            {
                return this.NotFound();
            }

            return this.Ok(enteredChart);
        }

        public IQueryable<EnteredChart> GetEnteredCharts()
        {
            return this.db.EnteredCharts;
        }

        // GET: api/EnteredChartsApi/5

        // PUT: api/EnteredChartsApi/5
        [ResponseType(typeof(EnteredChart))]
        public async Task<IHttpActionResult> PostEnteredChart(EnteredChart enteredChart)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            this.db.EnteredCharts.Add(enteredChart);
            await this.db.SaveChangesAsync();

            return this.CreatedAtRoute("DefaultApi", new { id = enteredChart.EnteredChartId }, enteredChart);
        }

        /// <summary>
        /// Puts the entered chart.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="enteredChart">The entered chart.</param>
        /// <returns></returns>
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEnteredChart(int id, EnteredChart enteredChart)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            if (id != enteredChart.EnteredChartId)
            {
                return this.BadRequest();
            }

            this.db.Entry(enteredChart).State = EntityState.Modified;

            try
            {
                await this.db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.EnteredChartExists(id))
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

        // POST: api/EnteredChartsApi

        // DELETE: api/EnteredChartsApi/5

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
        /// Determines whether the chart exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>True if the chart exists; false otherwise.</returns>
        private bool EnteredChartExists(int id)
        {
            return this.db.EnteredCharts.Count(e => e.EnteredChartId == id) > 0;
        }
    }
}