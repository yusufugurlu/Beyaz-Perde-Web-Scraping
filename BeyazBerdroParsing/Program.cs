using BeyazBerdroParsing.Dtos;
using BeyazBerdroParsing.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml;

class Program
{
    static async Task Main(string[] args)
    {
        string data = ReadData();

        var splitData = data.Replace("\r", "").Split('\n');

        foreach (var item in splitData)
        {
            if (!string.IsNullOrEmpty(item))
            {
                string filmName = item.Split('|')[0].Trim();
                string url = item.Split('|')[1].Trim();
                string id = GetFilmIdFromUrl(url);

                var comments = await GetFilmInComments(id);

                MovieDto movieDto = new()
                {
                    Name = filmName,
                    Comments = comments
                };

                await SaveMovie(movieDto);
            }
        }

    }



    private static async Task SaveMovie(MovieDto movieDto)
    {
        using (var context = new ApplicationDbContext())
        {
            var dateNow = DateTime.Now;

            Movie movie = new Movie()
            {
                CreatedDate = dateNow,
                MovieName = movieDto.Name
            };

            foreach (var comment in movieDto.Comments)
            {
                movie.Comments.Add(new FilmComment()
                {
                    Comment = comment.Comment,
                    CreatedDate = dateNow,
                    CommentDate = comment.CommentDate,
                    CommentLiked = comment.CommentLiked,
                    WriterName = comment.WriterName,
                    IsSpoiler = comment.IsSpoiler,
                });
            }


            // Ürünü veritabanına ekle
            await context.Movies.AddAsync(movie);
            var result = await context.SaveChangesAsync();
            if (result < 1)
            {
                Console.WriteLine("DB kaydedilemedi.");
            }
        }
    }
    private static async Task<List<CommentDto>> GetFilmInComments(string id)
    {
        int totalPages = int.MaxValue;
        int currentPage = 0;

        List<CommentDto> comments = new List<CommentDto>();

        // Tüm sayfaları dolaşacak şekilde istek yapma
        while (currentPage < totalPages)
        {

            string url = $"https://www.sinemalar.com/ajax/common/comments/{id}/0/null/{currentPage}/200/1";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, null);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        string html = await response.Content.ReadAsStringAsync();

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        if (currentPage == 0)
                        {
                            totalPages = GetPageSize(doc);
                        }

                        HtmlNodeCollection commentNodes = doc.DocumentNode.SelectNodes("//div[@class='card card-comment'][@data-user-up-voted='false']");

                        if (commentNodes != null)
                        {
                            var list = commentNodes.ToList();
                            foreach (HtmlNode h1Node in list)
                            {
                                HtmlDocument docComment = new HtmlDocument();
                                docComment.LoadHtml(h1Node.InnerHtml);

                                var userName = GetUserName(docComment);
                                var commentDate = GetCommentDate(docComment);
                                var comment = GetComment(docComment);
                                var liked = GetCommentLiked(docComment);
                                var isSpoiler = IsSpoiler(docComment);

                                comments.Add(new CommentDto()
                                {
                                    Comment = comment,
                                    CommentDate = commentDate,
                                    CommentLiked = liked,
                                    IsSpoiler = isSpoiler,
                                    WriterName = userName
                                });

                            }
                        }

                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("HTTP isteği sırasında hata oluştu: " + e.Message);
                    break;
                }
            }

            currentPage++;
            await Task.Delay(200);
        }

        return comments;
    }


    private static bool IsSpoiler(HtmlDocument doc)
    {
        HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='content-list']");

        if (divNode != null)
        {

            HtmlNode aNode = divNode.SelectSingleNode("//a[@class='btn btn-bordered btn-brand-border']");

            if (aNode != null)
            {
                Console.WriteLine("İçerik: " + aNode.InnerText.Replace("\n", "").Trim());
                return true;
            }

        }

        return false;
    }


    private static string GetCommentLiked(HtmlDocument doc)
    {
        HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@class='comment-actions']");

        if (divNode != null)
        {
            // div elementinin içindeki ilk a elementini seç
            HtmlNode aNode = divNode.SelectSingleNode(".//a");
            if (aNode != null)
            {
                // a elementinin içeriğini yazdır
                HtmlNode imgNode = aNode.SelectSingleNode("//img[@class='svg js-lazy-image']");

                if (imgNode != null)
                {
                    // img elementinin ardındaki metni yazdır
                    Console.WriteLine("İçerik: " + imgNode.NextSibling.InnerText.Replace("\n", "").Trim());
                    return imgNode.NextSibling.InnerText.Replace("\n", "").Trim();
                }
            }
        }

        return "";
    }

    private static string GetComment(HtmlDocument doc)
    {
        HtmlNode pNode = doc.DocumentNode.SelectSingleNode("//div[@class='content-list']/p");

        if (pNode != null)
        {
            Console.WriteLine("İçerik: " + pNode.InnerText.Trim());
            return pNode.InnerText.Replace("\n","").Replace("\r","").Trim();
        }

        return "";
    }

    private static string GetCommentDate(HtmlDocument doc)
    {
        HtmlNode spanNode = doc.DocumentNode.SelectSingleNode("//span[@class='comment-date']");

        if (spanNode != null)
        {
            Console.WriteLine("İçerik: " + spanNode.InnerText.Replace("\n", "").Trim());
            return spanNode.InnerText.Replace("\n", "").Trim();
        }

        return "";
    }

    private static string GetUserName(HtmlDocument doc)
    {
        HtmlNode spanNode = doc.DocumentNode.SelectSingleNode("//span[@class='comment-profile']/a");

        if (spanNode != null)
        {
            Console.WriteLine("İçerik: " + spanNode.InnerText.Replace("\n", "").Trim());
            return spanNode.InnerText.Replace("\n", "").Trim();
        }

        return "";
    }

    private static int GetPageSize(HtmlDocument doc)
    {
        HtmlNode ulNode = doc.DocumentNode.SelectSingleNode("//ul[@class='pager']");

        if (ulNode != null)
        {
            // ul elementinin içindeki son li elementini seç
            HtmlNode lastLiNode = ulNode.SelectSingleNode("./li[last()]");
            if (lastLiNode != null)
            {
                // İçeriği yazdır
                HtmlNode aNode = lastLiNode.SelectSingleNode(".//a");
                if (aNode != null)
                {
                    // onclick olayı özelliğini al
                    string onclickValue = aNode.GetAttributeValue("onclick", "");
                    Console.WriteLine("Son li Elementi <a> Etiketinin Onclick Değeri: " + onclickValue);
                    return GetCommentCountFromJavascript(onclickValue);
                }
            }
            else
            {
                Console.WriteLine("Belirtilen sınıfa sahip ul içinde li bulunamadı.");
            }
        }

        return 0;
    }

    static int GetCommentCountFromJavascript(string javascriptCode)
    {
        // Regex pattern'i tanımlıyoruz
        string pattern = @"javascript:getComments\(\d+,\d+,(\d+)\);";

        // Regex eşleşmesini yapıyoruz
        Match match = Regex.Match(javascriptCode, pattern);

        // Eşleşme kontrolü yapılıyor
        if (match.Success)
        {
            // Eşleşme bulunduysa grup değeri (comment count) döndürülüyor
            return int.Parse(match.Groups[1].Value);
        }
        else
        {
            // Eşleşme bulunamadıysa -1 döndürülüyor veya bir hata işlenebilir
            return -1;
        }
    }

    private static async Task GetFilmInformation(string url)
    {

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    HtmlNodeCollection h1Nodes = doc.DocumentNode.SelectNodes("//h1[@class='pr-new-br']");

                    if (h1Nodes != null)
                    {
                        foreach (HtmlNode h1Node in h1Nodes)
                        {
                            // İlgili h1 elementi içindeki span elementini seç
                            HtmlNode spanNode = h1Node.SelectSingleNode(".//span");
                            if (spanNode != null)
                            {
                                // İçeriği yazdır
                                Console.WriteLine("İçerik: " + spanNode.InnerText.Trim());
                                break;

                            }
                            else
                            {
                                Console.WriteLine("Belirtilen sınıfa sahip h1 içinde span bulunamadı.");
                            }
                        }
                    }


                    HtmlNodeCollection h1NodesPrice = doc.DocumentNode.SelectNodes("//div[@class='pr-bx-nm with-org-prc']");

                    if (h1NodesPrice != null)
                    {
                        foreach (HtmlNode h1Node in h1NodesPrice)
                        {
                            // İlgili h1 elementi içindeki span elementini seç
                            HtmlNode spanNode = h1Node.SelectSingleNode(".//span");
                            decimal.TryParse(spanNode.InnerText.Replace(" TL", ""), out decimal price);
                            if (spanNode != null)
                            {
                                // İçeriği yazdır
                                Console.WriteLine("İçerik: " + spanNode.InnerText.Trim());
                                break;

                            }
                            else
                            {
                                Console.WriteLine("Belirtilen sınıfa sahip h1 içinde span bulunamadı.");
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("HTTP isteği sırasında hata oluştu: " + e.Message);
            }
        }
    }

    static string GetFilmIdFromUrl(string url)
    {
        // Regex pattern'i tanımlıyoruz
        string pattern = @"\/film\/(\d+)\/";

        // Regex eşleşmesini yapıyoruz
        Match match = Regex.Match(url, pattern);

        // Eşleşme kontrolü yapılıyor
        if (match.Success)
        {
            // Eşleşme bulunduysa grup değeri (film ID'si) döndürülüyor
            return match.Groups[1].Value;
        }
        else
        {
            // Eşleşme bulunamadıysa boş döndürülüyor
            return string.Empty;
        }
    }

    private static string ReadData()
    {
        string dosyaYolu = "filmler.txt";

        string icerik = System.IO.File.ReadAllText(dosyaYolu);
        return icerik;
    }
}