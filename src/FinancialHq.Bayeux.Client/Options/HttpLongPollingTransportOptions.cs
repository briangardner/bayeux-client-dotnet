﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using FinancialHq.Bayeux.Client.Extensions;
using FinancialHq.Bayeux.Client.Transport;
using Microsoft.Extensions.Options;
using Polly;

namespace FinancialHq.Bayeux.Client.Options
{
    public class HttpLongPollingTransportOptions
    {
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
            if (HttpClient == null)
                throw new Exception("Please Set HttpClient.");

            return new HttpLongPollingTransport(new OptionsWrapper<HttpLongPollingTransportOptions>(new HttpLongPollingTransportOptions()
            {
                HttpClient = HttpClient,
                Uri = Uri
            }), new List<IExtension>(), Policy.NoOp());
        }        
    }
}