﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialHq.Bayeux.Client.Channels
{
    public class ChannelId
    {
        public const string WILD = "*";
        public const string DEEPWILD = "**";

        private readonly string _name;
        private readonly string[] _segments;
        private readonly int _wild;
        private readonly string _parent;

        public ChannelId(string name)
        {
            Wilds = new List<string>();
            if (string.IsNullOrEmpty(name) || !name.StartsWith("/") || name.Trim() == "/")
            {
                throw new ArgumentException(name);
            }

            _name = name;
            

            if (name.EndsWith("/"))
            {
                name = name.Substring(0, name.Length - 1 - 0);
            }

            _segments = name.Substring(1).Split('/');
            var wilds = new string[_segments.Length + 1];

            var b = new StringBuilder();
            b.Append('/');

            for (var i = 0; i < _segments.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(_segments[i]))
                {
                    throw new ArgumentException(name);
                }

                if (i > 0)
                {
                    b.Append(_segments[i - 1]).Append('/');
                }

                wilds[_segments.Length - i] = b + "**";
            }
            wilds[0] = b + "*";

            _parent = _segments.Length == 1 ? null : b.ToString().Substring(0, b.Length - 1);

            if (_segments.Length == 0)
            {
                _wild = 0;
            }
            else switch (_segments[_segments.Length - 1])
            {
                case WILD:
                    _wild = 1;
                    break;
                case DEEPWILD:
                    _wild = 2;
                    break;
                default:
                    _wild = 0;
                    break;
            }

            Wilds = _wild == 0 ? new List<string>(wilds).AsReadOnly() : new List<string>().AsReadOnly();
        }

        public IList<string> Wilds { get; }

        public bool Wild => _wild > 0;

        public bool DeepWild => _wild > 1;

        public bool IsMeta()
        {
            return _segments.Length > 0 && "meta".Equals(_segments[0]);
        }

        public bool IsService()
        {
            return _segments.Length > 0 && "service".Equals(_segments[0]);

        }

        public static bool IsMeta(string channelId)
        {
            return channelId != null && channelId.StartsWith("/meta/");
        }

        public static bool IsService(string channelId)
        {
            return channelId != null && channelId.StartsWith("/service/");
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is ChannelId id))
            {
                return false;
            }

            if (id.Depth() != Depth())
            {
                return false;
            }

            for (var i = id.Depth(); i-- > 0;)
            {
                if (!id.GetSegment(i).Equals(GetSegment(i)))
                {
                    return false;
                }
            }

            return true;

        }

        /* ------------------------------------------------------------ */
        /// <summary>Match channel IDs with wildcard support</summary>
        /// <param name="name">
        /// </param>
        /// <returns> true if this channelID matches the passed channel ID. If this channel is wild, then matching is wild.
        /// If the passed channel is wild, then it is the same as an equals call.
        /// </returns>
        public bool Matches(ChannelId name)
        {
            if (name.Wild)
            {
                return Equals(name);
            }

            switch (_wild)
            {

                case 0:
                    return Equals(name);

                case 1:
                    if (name._segments.Length != _segments.Length)
                    {
                        return false;
                    }

                    for (var i = _segments.Length - 1; i-- > 0;)
                    {
                        if (!_segments[i].Equals(name._segments[i]))
                        {
                            return false;
                        }
                    }

                    return true;


                case 2:
                    if (name._segments.Length < _segments.Length)
                    {
                        return false;
                    }

                    for (var i = _segments.Length - 1; i-- > 0;)
                    {
                        if (!_segments[i].Equals(name._segments[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override string ToString()
        {
            return _name;
        }

        public int Depth()
        {
            return _segments.Length;
        }

        /* ------------------------------------------------------------ */
        public bool IsAncestorOf(ChannelId id)
        {
            if (Wild || Depth() >= id.Depth())
            {
                return false;
            }

            for (var i = _segments.Length; i-- > 0;)
            {
                if (!_segments[i].Equals(id._segments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /* ------------------------------------------------------------ */
        public bool IsParentOf(ChannelId id)
        {
            if (Wild || Depth() != id.Depth() - 1)
            {
                return false;
            }

            for (var i = _segments.Length; i-- > 0;)
            {
                if (!_segments[i].Equals(id._segments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /* ------------------------------------------------------------ */
        public string GetParent()
        {
            return _parent;
        }

        public string GetSegment(int i)
        {
            return i > _segments.Length ? null : _segments[i];
        }
    }

}
