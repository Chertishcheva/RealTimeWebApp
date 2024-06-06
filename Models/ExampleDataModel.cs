using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeWebApp.Models
{
    public class ExampleDataModel
    {
        public long id { get; set; }
        public string timeOfUpload { get; set; }
        public int data { get; set; }

        public ExampleDataModel() { }

        public ExampleDataModel(int _id, String _timeOfUpload, int _data)
        {
            id = _id;
            timeOfUpload = _timeOfUpload;
            data = _data;
        }
    }
}
