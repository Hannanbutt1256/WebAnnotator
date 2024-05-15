using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebAnnotator.Models;
using PagedList;
using PagedList.Mvc;

using System.Linq.Dynamic.Core;


namespace WebAnnotator.Controllers
{
    public class NewsAnnotatesController : Controller
    {
        private annotate_dbEntities db = new annotate_dbEntities();
        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? page, int pageSize = 10, string searchBy = null)
        {
            var pageNumber = page ?? 1;

            var query = db.NewsAnnotates.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchBy))
            {
                query = query.Where(n => n.News.Contains(searchBy) || n.Category.Contains(searchBy));
            }

            var totalItems = query.Count();

            // Perform sorting and filtering dynamically based on user input
            query = query.OrderBy("News"); // Default sorting, you can change this

            var pagedList = new StaticPagedList<NewsAnnotate>(
                query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                pageNumber,
                pageSize,
                totalItems
            );

            ViewBag.PageSize = pageSize;
            ViewBag.SearchBy = searchBy; // To preserve the search term in the view

            return View(pagedList);
        }



        [Authorize(Roles = "Admin")]
        // GET: NewsAnnotates/Details/5
        public ActionResult Details(double? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsAnnotate newsAnnotate = db.NewsAnnotates.Find(id);
            if (newsAnnotate == null)
            {
                return HttpNotFound();
            }
            return View(newsAnnotate);
        }
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: NewsAnnotates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "News, Category, IsTrue")] NewsAnnotate newsAnnotate, string submitButton)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Ensure there are no null values
                    if (submitButton == "SubmitAll" && (newsAnnotate.News == null || newsAnnotate.Category == null || newsAnnotate.IsTrue == null))
                    {
                        ViewBag.ErrorMessage = "Please provide all data for News, Category, and IsTrue.";
                        return View(newsAnnotate);
                    }
                    else if (submitButton == "SubmitNews" && newsAnnotate.News == null)
                    {
                        ViewBag.ErrorMessage = "Please provide a news article in Urdu.";
                        return View(newsAnnotate);
                    }

                    // Find the last index in Id
                    var lastId = db.NewsAnnotates.OrderByDescending(n => n.Id).FirstOrDefault()?.Id ?? 0;

                    // Create a new entry with an automatically generated ID
                    var newNews = new NewsAnnotate
                    {
                        Id = lastId + 1,
                        News = newsAnnotate.News,
                        Category = newsAnnotate.Category,
                        IsTrue = newsAnnotate.IsTrue
                        // Set other properties if needed
                    };

                    db.NewsAnnotates.Add(newNews);
                    db.SaveChanges();

                    return RedirectToAction("Index", "Annotator");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while saving the news article.";
            }

            return View(newsAnnotate);
        }


        // GET: NewsAnnotates/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(double? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsAnnotate newsAnnotate = db.NewsAnnotates.Find(id);
            if (newsAnnotate == null)
            {
                return HttpNotFound();
            }

            // Retrieve categories for the dropdown list
            ViewBag.CategoryList = GetCategoriesSelectList(newsAnnotate.Category);

            return View(newsAnnotate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NewsAnnotate newsAnnotate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(newsAnnotate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Retrieve categories for the dropdown list
            ViewBag.CategoryList = GetCategoriesSelectList(newsAnnotate.Category);

            return View(newsAnnotate);
        }

        // Helper method to get predefined categories as SelectList
        private SelectList GetCategoriesSelectList(string selectedCategory)
        {
            var categories = new List<string> { "Business", "Entertainment", "Health", "Politics", "Science", "Sports", "Weird", "World" };

            var selectListItems = categories.Select(c => new SelectListItem
            {
                Value = c,
                Text = c,
                Selected = string.Equals(c, selectedCategory, StringComparison.OrdinalIgnoreCase)
            }).ToList();

            return new SelectList(selectListItems, "Value", "Text");
        }


        // GET: NewsAnnotates/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(double? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsAnnotate newsAnnotate = db.NewsAnnotates.Find(id);
            if (newsAnnotate == null)
            {
                return HttpNotFound();
            }
            return View(newsAnnotate);
        }

        // POST: NewsAnnotates/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(double id)
        {
            NewsAnnotate newsAnnotate = db.NewsAnnotates.Find(id);
            db.NewsAnnotates.Remove(newsAnnotate);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
