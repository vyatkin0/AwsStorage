using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AwsStorage.Infrastructure;
using AwsStorage.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AwsStorage.Controllers
{
    // Storage controller.
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        ILogger _logger;
        AwsProxy _awsProxy;
        public StorageController(ILogger<StorageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _awsProxy = new AwsProxy(_logger, configuration);
        }

        [HttpPost]
        public async Task<IActionResult> SetItem([FromBody][Required] KeyValuePair<string, string> item)
        {
            return Ok(await _awsProxy.AwsPutObject(item.Key, item.Value));
        }

        [HttpPost]
        public async Task<IActionResult> GetItem([FromBody][Required]string key)
        {
            return Ok(await _awsProxy.AwsGetObject(key));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem([FromBody][Required]string key)
        {
            return Ok(await _awsProxy.AwsDeleteObject(key));
        }

        [HttpPost]
        public async Task<IActionResult> List([FromBody][Required]ListModel model)
        {
            return Ok(await _awsProxy.AwsListObjects(model.Prefix, model.Limit));
        }

        [HttpPost]
        public async Task<IActionResult> Clear([FromBody][Required]string prefix)
        {
           return Ok(await _awsProxy.AwsDeleteObjects(prefix));
        }
    }
}
