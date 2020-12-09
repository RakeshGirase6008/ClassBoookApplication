using ClassBookApplication.Domain.Common;
using JW;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public class TestimonialModel : PageModel
    {
        public IList<Testimonial> Testimonials { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int MaxPages { get; set; }
        public Pager Pager { get; set; }

        //public void OnGet(int p = 1)
        //{
        //    // properties for pager parameter controls
        //    // generate list of sample items to be paged
        //    //var dummyItems = Enumerable.Range(1, TotalItems).Select(x => "Item " + x);
        //    var dummyTestimonial = _classBookService.GetAllTestimonials();
        //    // get pagination info for the current page
        //    Pager = new Pager(dummyTestimonial.Count(), p, PageSize, MaxPages);
        //    // assign the current page of items to the Items property
        //    //Testimonials = dummyItems.Skip((Pager.CurrentPage - 1) * Pager.PageSize).Take(Pager.PageSize);
        //    Testimonials = dummyTestimonial.Skip((Pager.CurrentPage - 1) * Pager.PageSize).Take(Pager.PageSize);
        //}
    }
}
