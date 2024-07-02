using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyazBerdroParsing.Models
{
    public class FilmComment: BaseEntity
    {
        public string WriterName { get; set; }
        public string Comment { get; set; }
        public string CommentDate { get; set; }
        public string CommentLiked { get; set; }
        public bool IsSpoiler { get; set; }

        public int MovieId { get; set; }
        public virtual Movie Movie { get; set; }
    }
}
