﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Silmoon.Data.MongoDB.Extensions
{
    public static class BsonDocumentExtension
    {
        public static T ToObject<T>(this BsonDocument document) where T : new()
        {
            return BsonSerializer.Deserialize<T>(document);
        }


        public static JObject ToJObject(this BsonDocument document)
        {
            return JObject.Parse(document.ToJson());
        }

        public static BsonDocument ToBsonDocument(this JObject jObject)
        {
            return BsonDocument.Parse(jObject.ToString());
        }
        public static JArray ToJArray(this BsonArray document)
        {
            return JArray.Parse(document.ToJson());
        }
        public static BsonArray ToBsonArray(this JArray jArray)
        {
            var result = new BsonArray();
            foreach (var item in jArray)
            {
                if (item is JObject)
                    result.Add(((JObject)item).ToBsonDocument());
                else if (item is JArray)
                {
                    BsonArray bsonArray = new BsonArray();
                    foreach (var arrayItem in item)
                    {
                        var val = BsonValue.Create(arrayItem.ToString());
                        bsonArray.Add(val);
                    }
                    result.Add(bsonArray);
                }
                else
                    result.Add(BsonValue.Create(item.ToString()));
            }
            return result;
        }
    }
}
