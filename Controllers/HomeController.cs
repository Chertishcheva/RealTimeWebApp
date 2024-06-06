using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealTimeWebApp.Models;
using RealTimeWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly SourceDataService source;

        private List<ExampleDataModel> newData;

        public HomeController(ILogger<HomeController> _logger, SourceDataService _source) {
            logger = _logger;
            source = _source;
        }

        [Route("/updateData")]
        public IActionResult showAllData()
        {
            newData = source.getAll();

            return PartialView(newData);
        }
    }
}
