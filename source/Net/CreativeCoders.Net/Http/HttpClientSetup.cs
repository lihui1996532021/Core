﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using CreativeCoders.Core;
using CreativeCoders.Core.Collections;
using Microsoft.Extensions.Http;

namespace CreativeCoders.Net.Http;

public class HttpClientSetup : IHttpClientSetup
{
    private readonly IList<Action<HttpClientFactoryOptions>> _configureActions;

    public HttpClientSetup()
    {
        _configureActions = new List<Action<HttpClientFactoryOptions>>();
    }

    public IHttpClientSetup ConfigureClient(Action<HttpClient> configureClient)
    {
        Ensure.NotNull(configureClient, nameof(configureClient));

        _configureActions.Add(options => options.HttpClientActions.Add(configureClient));

        return this;
    }

    public IHttpClientSetup ConfigureClientHandler(Func<HttpClientHandler> configureClientHandler)
    {
        Ensure.NotNull(configureClientHandler, nameof(configureClientHandler));

        _configureActions.Add(options =>
            options.HttpMessageHandlerBuilderActions.Add(builder =>
                builder.PrimaryHandler = configureClientHandler()));

        return this;
    }

    public void InvokeSetup(HttpClientFactoryOptions options)
    {
        _configureActions.ForEach(configure => configure(options));
    }
}