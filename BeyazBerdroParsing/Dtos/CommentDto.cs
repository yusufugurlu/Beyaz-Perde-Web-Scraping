using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyazBerdroParsing.Dtos
{
    public class CommentDto
    {
        public string WriterName { get; set; }
        public string Comment { get; set; }
        public string CommentDate { get; set; }
        public string CommentLiked { get; set; }
        public bool IsSpoiler { get; set; }
    }
}
