using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using TablutBackend.Models;

namespace TablutBackend
{
    public class CacheTT
    {
        private static Cache _cache = null;
        private static Cache cache
        {

            get
            {

                if (_cache == null)
                    _cache = (HttpContext.Current == null) ? HttpRuntime.Cache : HttpContext.Current.Cache;

                return _cache;
            }
            set
            {
                _cache = value;
            }


        }

        public static Move Get(string key)
        {
            return (Move)cache.Get(key);
        }

        public static void Add(string key, Move value)
        {
            //CacheItemPriority priority = CacheItemPriority.NotRemovable;
            //var expiration = TimeSpan.FromMinutes(10);
            cache.Insert(key, value);
        }

        public static void Remove(string key)
        {
            cache.Remove(key);
        }
    }
}