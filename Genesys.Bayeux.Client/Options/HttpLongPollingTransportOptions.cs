﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Genesys.Bayeux.Client.Connectivity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Genesys.Bayeux.Client.Options
{
    public class HttpLongPollingTransportOptions
    {
        /// <summary>
        /// An HTTP POST implementation. It should not do HTTP pipelining (rarely done for POSTs anyway).
        /// See https://docs.cometd.org/current/reference/#_two_connection_operation.
        /// 
        /// <para>
        /// Set this property or HttpClient, but not both.
        /// </para>
        /// 
        /// <para>
        /// Enables the implementation of retry policies; useful for servers that may occasionally need a session refresh. Retries are general not supported by HttpClient, as (for some versions) SendAsync disposes the content of HttpRequestMessage. This means that a failed SendAsync call can't be retried, as the HttpRequestMessage can't be reused.
        /// </para>
        /// </summary>
        public IHttpPost HttpPost;

        /// <summary>
        /// HttpClient to use.
        /// 
        /// <para>
        /// Set this property or HttpPost, but not both.
        /// </para>
        /// </summary>
        public HttpClient HttpClient;

        public string Uri { get; set; }

        internal HttpLongPollingTransport Build()
        {
            if (Uri == null)
                throw new Exception("Please set Uri.");

            if (HttpPost == null)
            {
                return new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(
                    new HttpLongPollingTransportOptions()
                    {
                        HttpPost = new HttpClientHttpPost(HttpClient ?? new HttpClient()),
                        Uri = Uri
                    }));
            }
            else
            {
                if (HttpClient != null)
                    throw new Exception("Set HttpPost or HttpClient, but not both.");

                return new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
                {
                    HttpPost = HttpPost,
                    Uri = Uri
                }));
            }
        }        
    }
}