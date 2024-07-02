using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyazBerdroParsing.Dtos
{
    public class MovieDto
    {
        public string Name { get; set; }
        public List<CommentDto> Comments { get; set; }
    }
}
