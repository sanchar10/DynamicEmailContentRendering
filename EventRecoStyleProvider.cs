using System.Net.NetworkInformation;

namespace LiveEmailContentService
{
    public class EventRecoStyleProvider
    {
        public static string ReadImageRenderConfig()
        {
            // Specify the path to your JSON file
            string filePath = "EventRecoStyle.json";

            // Read the JSON file contents into a string
            return File.ReadAllText(filePath);
        }    

    }
}
