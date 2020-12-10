using ClassBookApplication.Domain.Common;
using System.Collections.Generic;

namespace ClassBookApplication.Models.PublicModel
{
    public class TestimonialModel : CommonPageModel
    {
        public IList<Testimonial> Testimonials { get; set; }
        
    }
}
