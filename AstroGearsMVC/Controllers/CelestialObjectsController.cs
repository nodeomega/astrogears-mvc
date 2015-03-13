// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CelestialObjectsController.cs" company="Jonathan Russell">
//   Copyright (c) Jonathan Russell - All Rights Reserved
// </copyright>
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
    ///     The Celestial Objects Controller.
    /// </summary>
    public class CelestialObjectsController : Controller
    {
        #region Fields

        /// <summary>
        /// The database.
        /// </summary>
        private readonly AstroGearsEntities db = new AstroGearsEntities();

        #endregion

        // GET: CelestialObjects

        // GET: CelestialObjects/Create
        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.CelestialObjectTypeId = new SelectList(
                this.db.CelestialObjectTypes, 
                "CelestialObjectTypeId", 
                "CelestialObjectTypeName");
            return this.View();
        }

        // POST: CelestialObjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="celestialObject">The celestial object.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">celestialObject is null</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([NotNull, Bind(Include = "CelestialObjectId,CelestialObjectName,CelestialObjectTypeId,AllowableOrb,Keywords,Draconic")] CelestialObject celestialObject)
        {
            if (celestialObject == null)
            {
                throw new ArgumentNullException("celestialObject");
            }

            if (this.ModelState.IsValid)
            {
                this.db.CelestialObjects.Add(celestialObject);
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.CelestialObjectTypeId = new SelectList(
                this.db.CelestialObjectTypes, 
                "CelestialObjectTypeId", 
                "CelestialObjectTypeName", 
                celestialObject.CelestialObjectTypeId);
            return this.View(celestialObject);
        }

        /// <summary>
        /// Creates the new celestial object.
        /// </summary>
        /// <param name="celestialObjectName">Name of the celestial object.</param>
        /// <param name="celestialObjectTypeId">The celestial object type identifier.</param>
        /// <param name="allowableOrb">The allowable orb.</param>
        /// <returns>
        /// The <see cref="Task" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">celestialObjectName is null</exception>
        [NotNull]
        public JsonResult CreateNewCelestialObject([NotNull] string celestialObjectName, byte celestialObjectTypeId, decimal allowableOrb)
        {
            if (celestialObjectName == null)
            {
                throw new ArgumentNullException("celestialObjectName");
            }

            if (!this.ModelState.IsValid)
            {
                return this.Json(0, JsonRequestBehavior.DenyGet);
            }

            var newCelestialObject = new CelestialObject
                                         {
                                             CelestialObjectName = celestialObjectName,
                                             CelestialObjectTypeId = celestialObjectTypeId,
                                             AlternateName = null,
                                             AllowableOrb = allowableOrb,
                                             Draconic = false,
                                             Keywords = string.Empty
                                         };

            this.db.CelestialObjects.Add(newCelestialObject);
            this.db.SaveChanges();

            var thisObject = this.db.CelestialObjects.FirstOrDefault(x => x.CelestialObjectName == celestialObjectName);

            return this.Json((thisObject != null) ? thisObject.CelestialObjectId : 0, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Checks if the celestial object exists.
        /// </summary>
        /// <param name="celestialObjectName">Name of the celestial object.</param>
        /// <returns>0 if not existent, 1 otherwise.  (Maybe more if something's screwed up)</returns>
        /// <exception cref="System.ArgumentNullException">celestialObjectName is null.</exception>
        [NotNull]
        public JsonResult CelestialObjectExists([NotNull] string celestialObjectName)
        {
            if (celestialObjectName == null)
            {
                throw new ArgumentNullException("celestialObjectName");
            }

            return this.Json(
                this.db.CelestialObjects.Count(x => x.CelestialObjectName == celestialObjectName),
                JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the Celestial Object's Id.
        /// </summary>
        /// <param name="celestialObjectName">Name of the celestial object.</param>
        /// <returns>The Id.</returns>
        /// <exception cref="System.ArgumentNullException">celestialObjectName is null.</exception>
        [NotNull]
        public JsonResult GetCelestialObjectId([NotNull] string celestialObjectName)
        {
            if (celestialObjectName == null)
            {
                throw new ArgumentNullException("celestialObjectName");
            }

            return this.Json(
                this.db.CelestialObjects.First(x => x.CelestialObjectName == celestialObjectName).CelestialObjectId,
                JsonRequestBehavior.AllowGet);
        }

        // GET: CelestialObjects/Edit/5

        // GET: CelestialObjects/Delete/5

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CelestialObject celestialObject = await this.db.CelestialObjects.FindAsync(id);
            if (celestialObject == null)
            {
                return this.HttpNotFound();
            }

            return this.View(celestialObject);
        }

        // POST: CelestialObjects/Delete/5

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CelestialObject celestialObject = await this.db.CelestialObjects.FindAsync(id);
            this.db.CelestialObjects.Remove(celestialObject);
            await this.db.SaveChangesAsync();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CelestialObject celestialObject = await this.db.CelestialObjects.FindAsync(id);
            if (celestialObject == null)
            {
                return this.HttpNotFound();
            }

            return this.View(celestialObject);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CelestialObject celestialObject = await this.db.CelestialObjects.FindAsync(id);
            if (celestialObject == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.CelestialObjectTypeId = new SelectList(
                this.db.CelestialObjectTypes, 
                "CelestialObjectTypeId", 
                "CelestialObjectTypeName", 
                celestialObject.CelestialObjectTypeId);
            return this.View(celestialObject);
        }

        // POST: CelestialObjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="celestialObject">The celestial object.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">celestialObject is null</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([NotNull, Bind(Include = "CelestialObjectId,CelestialObjectName,CelestialObjectTypeId,AllowableOrb,Keywords,Draconic")] CelestialObject celestialObject)
        {
            if (celestialObject == null)
            {
                throw new ArgumentNullException("celestialObject");
            }

            if (this.ModelState.IsValid)
            {
                this.db.Entry(celestialObject).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.CelestialObjectTypeId = new SelectList(
                this.db.CelestialObjectTypes, 
                "CelestialObjectTypeId", 
                "CelestialObjectTypeName", 
                celestialObject.CelestialObjectTypeId);
            return this.View(celestialObject);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            var celestialObjects = this.db.CelestialObjects.Include(c => c.CelestialObjectType);
            return this.View(await celestialObjects.ToListAsync());
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