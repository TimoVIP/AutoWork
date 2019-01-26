using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WebApplication1.Controllers
{
    public class DefaultApiController : ApiController
    {
        /// <summary>
        /// http://localhost:47540/api/defaultapi/index
        /// 
        /// post only
        /// </summary>
        /// <returns></returns>
        public string Index()
        {
            return new JavaScriptSerializer().Serialize(new { Result = false, Msg = "错误的请求类型", Data = "null" });
        }


    }
}
