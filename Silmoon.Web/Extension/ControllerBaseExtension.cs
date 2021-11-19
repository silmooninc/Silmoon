﻿using Silmoon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Silmoon.Web.Extension
{
    public static class ControllerBaseExtension
    {

        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, 0, "", null, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, string Message, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, 0, Message, null, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, int Code, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, Code, "", null, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, int Code, string Message, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, Code, Message, null, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, object Data, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, 0, "", Data, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, string Message, object Data, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, 0, Message, Data, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, int Code, object Data, bool AllowGet = false)
        {
            return JsonStateFlag(controller, Success, Code, "", Data, AllowGet);
        }
        public static JsonResult JsonStateFlag(this ControllerBase controller, bool Success, int Code, string Message, object Data, bool AllowGet = false)
        {
            var result = new JsonResult();
            result.Data = StateFlag.Create(Success, Code, Message, Data);
            result.JsonRequestBehavior = AllowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet;
            result.ContentType = "application/json";
            result.ContentEncoding = Encoding.UTF8;
            return result;
        }

        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, 0, "", default, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, string Message, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, 0, Message, default, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, Code, "", default, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, string Message, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, Code, Message, default, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, T Data, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, 0, "", Data, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, string Message, T Data, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, 0, Message, Data, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, T Data, bool AllowGet = false)
        {
            return JsonStateFlag<T>(controller, Success, Code, "", Data, AllowGet);
        }
        public static JsonResult JsonStateFlag<T>(this ControllerBase controller, bool Success, int Code, string Message, T Data, bool AllowGet = false)
        {
            var result = new JsonResult();
            result.Data = StateFlag<T>.Create(Success, Code, Message, Data);
            result.JsonRequestBehavior = AllowGet ? JsonRequestBehavior.AllowGet : JsonRequestBehavior.DenyGet;
            result.ContentType = "application/json";
            result.ContentEncoding = Encoding.UTF8;
            return result;
        }

    }
}
