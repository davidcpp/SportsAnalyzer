using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsAnalyzer.Controllers
{
  public class StatsController : ApiController
  {
    static int[][] datasets = new int[][]
    {
      new int[] { 5, 15, 25, 30, 20, 5 },
      new int[] { 7, 13, 27, 28, 18, 7 },
      new int[] { 9, 11, 21, 34, 22, 3 }
    };

    [HttpGet]
    public IHttpActionResult GetDataset(int id)
    {
      if (id >= 0 && id < 3)
      {
        return Ok(datasets[id]);
      }
      else
      {
        return NotFound();
      }

    }
  }
}
