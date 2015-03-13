// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HouseAnglesController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AstroGearsMVC.Controllers
{
    using System;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using AstroGearsMVC.Models;

    using JetBrains.Annotations;

    /// <summary>
    /// The house angles controller.
    /// </summary>
    public class HouseAnglesController : Controller
    {
        #region Fields

        /// <summary>
        /// The database.
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        #endregion

        // GET: HouseAngles

        // GET: HouseAngles/Details/5
        #region Public Methods and Operators

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Details(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var houseAngle = await this.db.HouseAngles.FindAsync(id);

            if (houseAngle == null)
            {
                return this.HttpNotFound();
            }

            return this.View(houseAngle);
        }

        // GET: HouseAngles/Edit/5

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var houseAngle = await this.db.HouseAngles.FindAsync(id);

            if (houseAngle == null)
            {
                return this.HttpNotFound();
            }

            return this.View(houseAngle);
        }

        // POST: HouseAngles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="houseAngle">
        /// The house angle.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([NotNull, Bind(Include = "HouseId,HouseName,HouseAngleName")] HouseAngle houseAngle)
        {
            if (houseAngle == null)
            {
                throw new ArgumentNullException("houseAngle");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(houseAngle);
            }

            this.db.Entry(houseAngle).State = EntityState.Modified;
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            return this.View(await this.db.HouseAngles.ToListAsync());
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
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