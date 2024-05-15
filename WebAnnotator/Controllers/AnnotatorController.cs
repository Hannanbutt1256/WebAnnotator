using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAnnotator.Models;


namespace WebAnnotator.Controllers
{
    public class AnnotatorController : Controller
    {
        private annotate_dbEntities db = new annotate_dbEntities();

        // GET: Annotator
        [Authorize]
        public ActionResult Index()
        {
            // Get a shuffled list of news items that are not annotated yet
            var unannotatedNewsList = db.NewsAnnotates
                .Where(n => string.IsNullOrEmpty(n.Category) && n.IsTrue == null)
                .OrderBy(n => Guid.NewGuid())
                .ToList();

            if (!unannotatedNewsList.Any())
            {
                // All news items are annotated
                ViewBag.Message = "No unannotated news available.";
                return View();
            }

            // Get the first (random) unannotated news item from the shuffled list
            var unannotatedNews = unannotatedNewsList.First();

            return View(unannotatedNews);
        }

        // POST: Annotator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(int id, string category, bool? isTrue)
        {
            // Retrieve the news item by Id
            var newsAnnotate = db.NewsAnnotates.Find(id);

            if (newsAnnotate != null)
            {
                // Check if feedback is provided
                if (category != null && isTrue.HasValue)
                {
                    // Update the news item with user feedback
                    newsAnnotate.Category = category;
                    newsAnnotate.IsTrue = isTrue.HasValue ? isTrue.ToString() : null;

                    db.SaveChanges();
                }
                else
                {
                    // Redirect to the same news item with an error message
                    ViewBag.ErrorMessage = "Please provide feedback for both Category and 'Is True?' fields.";
                    return View(newsAnnotate);
                }
            }

            // Redirect to the next unannotated news item
            return RedirectToAction("Index");
        }
    }
}
