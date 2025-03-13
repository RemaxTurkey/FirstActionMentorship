using System;

namespace Application.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheAttribute(string keyTemplate, int cacheTime = 1800) : Attribute
    {
        public string KeyTemplate { get; set; } = keyTemplate;
        public int CacheTime { get; set; } = cacheTime;
    }
} 