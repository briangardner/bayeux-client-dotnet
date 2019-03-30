using System;
using System.Collections.Generic;
using System.Linq;

namespace Genesys.Bayeux.Client
{
    public class ChannelId
    {
        private readonly string _id;
        private readonly List<string> _segments;
        private readonly List<string> _wildcards;

        public ChannelId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "ChannelId cannot be null or empty string");
            }

            if (!id.StartsWith("/"))
            {
                throw new ArgumentException("ChannelId must start with '/'");
            }
            if (id =="/")
            {
                throw new ArgumentException("ChannelId must have something after initial '/'");
            }
            
            id = id.Trim();
            if (id.EndsWith("/"))
            {
                id = id.Substring(0, id.Length - 1);
            }
            
            _id = id;
            var segments = _id.Split('/').ToList();
            foreach(var segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment)){
                    throw new ArgumentException("Segment of URL cannot be empty between '/'.");
                }

            }
        }

        public static bool IsMeta(String channelId)
        {
            return channelId != null && channelId.StartsWith("/meta/");
        }

        public static bool IsService(String channelId)
        {
            return channelId != null && channelId.StartsWith("/service/");
        }

        public static bool IsBroadcast(String channelId)
        {
            return !IsMeta(channelId) && !IsService(channelId);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (!(obj is ChannelId)) 
            return false;

            var that = (ChannelId)obj;
            return _id.Equals(that._id);
        }

        public override string ToString()
        {
            return _id;
        }

        public bool IsWild()
        {
            return _segments.Last().EndsWith("*");
        }

        public bool IsDeepWild()
        {
            return _segments.Last().Equals("**");
        }
        public bool IsShallowWild()
        {
            return IsWild() && !IsDeepWild();
        }

        public string GetSegment(int i)
        {
            if (i >= 0 && i < _segments.Count)
            {
                return _segments[i];
            }

            throw new IndexOutOfRangeException("Attempting to access a segment out of range for this Channel");
        }

    }
}
