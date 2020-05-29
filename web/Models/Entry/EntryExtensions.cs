using System;
using System.Collections.Generic;
using Contentstack.Core;

namespace web.Models.Entry
{
    public class EntryExtensions
    {
        private static string REPOSITORY_KEY = typeof(EntryExtensions) + ".Repository";
        private static string CONTENT_TYPE_KEY = typeof(EntryExtensions) + ".ContentType";

        public static void Initialize(Contentstack.Core.Models.Entry entry, ContentstackClient client, string contentType)
        {
            if (entry.Metadata == null || !entry.Metadata.ContainsKey(REPOSITORY_KEY))
            {
                entry.Metadata = new Dictionary<string, object>();
            }

            entry.Metadata[REPOSITORY_KEY] = contentType;
            entry.Metadata[CONTENT_TYPE_KEY] = contentType;
        }

        public static string GetContentType(Contentstack.Core.Models.Entry entry)
        {
            if (entry.Metadata == null || !entry.Metadata.ContainsKey(CONTENT_TYPE_KEY))
            {
                throw new ApplicationException(entry.Uid + " does not specify " + CONTENT_TYPE_KEY + " : " + entry.Metadata);
            }

            return entry.Metadata[CONTENT_TYPE_KEY].ToString();
        }

        public static ContentstackClient GetRepository(Contentstack.Core.Models.Entry entry)
        {
            if (entry.Metadata == null || !entry.Metadata.ContainsKey(REPOSITORY_KEY))
            {
                throw new ApplicationException(entry.Uid + " does not specify " + REPOSITORY_KEY + " : " + entry.Metadata);
            }

            return entry.Metadata[REPOSITORY_KEY] as ContentstackClient;
        }
    }
}
