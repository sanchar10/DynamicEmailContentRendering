
using System.Drawing;

using System.Drawing.Imaging;
using System.Net;

using LiveEmailContentService;
using LiveImageForEmail;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;



//Call: /api/image/generate?imageUrl=https://example.com/image.jpg&text=Hello,%20World!

[Route("api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{   

    [HttpGet("RedirectToClickedURL")]
    public IActionResult RedirectToClickedURL()
    {
        string pluginId = (string)HttpContext.Request.Query["pluginId"];
        string customerId = (string)HttpContext.Request.Query["customerId"];
        string scenarioId = (string)HttpContext.Request.Query["scenarioId"];
        bool hasIndex = int.TryParse(HttpContext.Request.Query["index"], out int index);

        // Get all query parameters as a Dictionary
        Dictionary<string, string> queryParams = HttpContext.Request.Query
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        //Get the data
        JObject dataObject = JObject.Parse(EventRecoDataProvider.FetchData(queryParams));

        //Parse style JSON
        JObject styleObject = JObject.Parse(EventRecoStyleProvider.ReadImageRenderConfig());

        string ctaLinkPath = styleObject["imageCTA"].ToString();

        //replace path * with requested index
        ctaLinkPath = ctaLinkPath.Replace("*", index.ToString());

        // Read the element at the specified path from dataJson
        string ctaLink = dataObject.SelectToken(ctaLinkPath).ToString();

        return Redirect(ctaLink);
    }


    [HttpGet("ImageGen")]
    public IActionResult GenerateImage()
    {
        string pluginId = (string)HttpContext.Request.Query["pluginId"];
        string customerId = (string)HttpContext.Request.Query["customerId"];
        string scenarioId = (string)HttpContext.Request.Query["scenarioId"];
        bool hasIndex = int.TryParse(HttpContext.Request.Query["index"], out int index);

        Dictionary<string, string> queryParams = HttpContext.Request.Query
           .ToDictionary(q => q.Key, q => q.Value.ToString());

        //Get the data
        JObject dataObject = JObject.Parse(EventRecoDataProvider.FetchData(queryParams));

        //Parse style JSON
        JObject renderConfig = JObject.Parse(EventRecoStyleProvider.ReadImageRenderConfig());

        //Create empty image
        int width = (int)renderConfig["baseImage"]["width"]; 
        int height = (int)renderConfig["baseImage"]["height"];


        Bitmap image = new Bitmap(width, height);
        //Consider Using a Higher Bit Depth:
        //Bitmap image = new Bitmap(width, height, PixelFormat.Format32bppArgb);


        using (Graphics graphics = Graphics.FromImage(image))
        {
            // Set background color
            graphics.Clear(ColorTranslator.FromHtml(renderConfig["baseImage"]["color"].ToString()));

            //graphics.SmoothingMode = SmoothingMode.HighQuality;
            //Use a Higher Quality Interpolation for Image Scaling
            //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //Anti-Aliasing to smooth out edges and improve the overall visual quality.You can enable anti-aliasing for text rendering and drawing shapes.
            //graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            //graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //graphics.DpiX = 300; // Set a higher DPI value
            //graphics.DpiY = 300; // Set a higher DPI value

            // Iterate through style elements and apply styles to the data
            foreach (var styleElement in renderConfig["imageRender"])
            {
                string elementType = styleElement["type"].ToString();

                // Read the desired element
                string elementValue = styleElement["value"].ToString();

                string dataValue;

                //replace path * with requested index
                if (elementValue.StartsWith('$'))
                {
                    string path = elementValue.Replace("*", index.ToString());
                    dataValue = dataObject.SelectToken(path).ToString();
                }
                else
                {
                    dataValue = elementValue;
                }

                // Create a graphics object and draw with the specified styles
                //write a switch case with elementType

                switch (elementType)
                {
                    case "text":
                        DrawText(graphics,
                        dataValue,
                        styleElement["fontType"].ToString(),
                        int.Parse(styleElement["fontSize"].ToString().Replace("px", "")),
                        ColorTranslator.FromHtml(styleElement["textColor"].ToString()),
                        int.Parse(styleElement["positionX"].ToString()),
                        int.Parse(styleElement["positionY"].ToString()),
                        int.Parse(styleElement["blockWidth"].ToString()),
                         int.Parse(styleElement["blockHeight"].ToString())
                        );
                        break;
                    case "button":
                        DrawButton(graphics,
                        dataValue,
                        styleElement["fontType"].ToString(),
                        int.Parse(styleElement["fontSize"].ToString().Replace("px", "")),
                        ColorTranslator.FromHtml(styleElement["textColor"].ToString()),
                        ColorTranslator.FromHtml(styleElement["buttonColor"].ToString()),
                        int.Parse(styleElement["positionX"].ToString()),
                        int.Parse(styleElement["positionY"].ToString()),
                        int.Parse(styleElement["blockWidth"].ToString()),
                         int.Parse(styleElement["blockHeight"].ToString())
                        );
                        break;
                    default:
                        break;
                }

            }
        }

        // Stream the image to the response
        MemoryStream stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);
        return File(stream, "image/jpeg");
    }



    Bitmap DownloadImage(string imageUrl)
    {
        using (WebClient webClient = new WebClient())
        {
            byte[] data = webClient.DownloadData(imageUrl);
            using (var stream = new MemoryStream(data))
            {
                return new Bitmap(stream);
            }
        }
    }

    static void DrawText(Graphics graphics, string text, string fontType, int fontSize, Color textColor, int positionX, int positionY, int blockWidth, int blockHeight)
    {
        using (Font font = new Font(fontType, fontSize))
        using (SolidBrush textBrush = new SolidBrush(textColor))
        {
            // Draw the text on the rectangle with center alignment
            graphics.DrawString(text, font, textBrush, new Rectangle(positionX, positionY, blockWidth, blockHeight));
        }
    }

    static void DrawButton(Graphics graphics, string text, string fontType, int fontSize, Color textColor, Color backgroundColor, int positionX, int positionY, int blockWidth, int blockHeight)
    {
        // Fill the rectangle with the specified background color
        graphics.FillRectangle(new SolidBrush(backgroundColor), positionX, positionY, blockWidth, blockHeight);

        using (Font font = new Font(fontType, fontSize))
        using (SolidBrush textBrush = new SolidBrush(textColor))
        {
            // Draw the text on the rectangle
            // Create StringFormat with center alignment
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            // Draw the text on the rectangle with center alignment
            graphics.DrawString(text, font, textBrush, new Rectangle(positionX, positionY, blockWidth, blockHeight), stringFormat);
        }
    }
}