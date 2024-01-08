using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LiveImageForEmail
{
    public class EventRecoDataProvider
    {
        
        public static string FetchData(Dictionary<string, string> parameters)        
        { 
            //Assume and API call, returning preset results for the POC           
          
            string filePath = "EventData2.json";
            
            // Read the JSON file contents into a string
            return File.ReadAllText(filePath);
        } 
    }
}
