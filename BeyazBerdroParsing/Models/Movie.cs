using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyazBerdroParsing.Models
{
    public class Movie: BaseEntity
    {
        public Movie()
        {
            Comments = new List<FilmComment>();
        }
        public string MovieName { get; set; }

        public List<FilmComment> Comments { get; set; }
    }
}
