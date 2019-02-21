﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Steeltoe.Management.Endpoint.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Steeltoe.Management.Endpoint.Env.Test
{
    public class EnvEndpointTest : BaseTest
    {
        [Fact]
        public void Constructor_ThrowsIfNulls()
        {
            IEnvOptions options = null;
            IConfiguration configuration = null;
            IHostingEnvironment env = null;

            Assert.Throws<ArgumentNullException>(() => new EnvEndpoint(options, configuration, env));

            options = new EnvOptions();
            Assert.Throws<ArgumentNullException>(() => new EnvEndpoint(options, configuration, env));

            configuration = new ConfigurationBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new EnvEndpoint(options, configuration, env));
        }

        [Fact]
        public void GetPropertySourceName_ReturnsExpected()
        {
            var opts = new EnvEndpointOptions();
            var builder = new ConfigurationBuilder();
            builder.AddEnvironmentVariables();
            var config = builder.Build();
            var ep = new EnvEndpoint(opts, config, new TestHosting());

            var provider = config.Providers.Single();
            string name = ep.GetPropertySourceName(provider);
            Assert.Equal(provider.GetType().Name, name);

            builder = new ConfigurationBuilder();
            builder.AddJsonFile("foobar", true);
            config = builder.Build();

            ep = new EnvEndpoint(opts, config, new TestHosting());

            provider = config.Providers.Single();
            name = ep.GetPropertySourceName(provider);
            Assert.Equal("JsonConfigurationProvider: [foobar]", name);
        }

        [Fact]
        public void GetPropertySourceDescriptor_ReturnsExpected()
        {
            var opts = new EnvEndpointOptions();
            var appsettings = new Dictionary<string, string>()
            {
                ["management:endpoints:enabled"] = "false",
                ["management:endpoints:path"] = "/cloudfoundryapplication",
                ["management:endpoints:loggers:enabled"] = "false",
                ["management:endpoints:heapdump:enabled"] = "true",
                ["management:endpoints:heapdump:sensitive"] = "true",
                ["management:endpoints:cloudfoundry:validatecertificates"] = "true",
                ["management:endpoints:cloudfoundry:enabled"] = "true"
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();

            var ep = new EnvEndpoint(opts, config, new TestHosting());
            var provider = config.Providers.Single();
            var desc = ep.GetPropertySourceDescriptor(provider, config);

            Assert.Equal("MemoryConfigurationProvider", desc.Name);
            var props = desc.Properties;
            Assert.NotNull(props);
            Assert.Equal(7, props.Count);
            Assert.Contains("management:endpoints:enabled", props.Keys);
            var prop = props["management:endpoints:enabled"];
            Assert.NotNull(prop);
            Assert.Equal("false", prop.Value);
            Assert.Null(prop.Origin);
        }

        [Fact]
        public void GetPropertySources_ReturnsExpected()
        {
            var opts = new EnvEndpointOptions();
            var appsettings = new Dictionary<string, string>()
            {
                ["management:endpoints:enabled"] = "false",
                ["management:endpoints:path"] = "/cloudfoundryapplication",
                ["management:endpoints:loggers:enabled"] = "false",
                ["management:endpoints:heapdump:enabled"] = "true",
                ["management:endpoints:cloudfoundry:validatecertificates"] = "true",
                ["management:endpoints:cloudfoundry:enabled"] = "true"
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();

            var ep = new EnvEndpoint(opts, config, new TestHosting());
            var result = ep.GetPropertySources(config);
            Assert.NotNull(result);
            Assert.Single(result);

            var desc = result[0];

            Assert.Equal("MemoryConfigurationProvider", desc.Name);
            var props = desc.Properties;
            Assert.NotNull(props);
            Assert.Equal(6, props.Count);
            Assert.Contains("management:endpoints:cloudfoundry:validatecertificates", props.Keys);
            var prop = props["management:endpoints:cloudfoundry:validatecertificates"];
            Assert.NotNull(prop);
            Assert.Equal("true", prop.Value);
            Assert.Null(prop.Origin);
        }

        [Fact]
        public void DoInvoke_ReturnsExpected()
        {
            var opts = new EnvEndpointOptions();
            var appsettings = new Dictionary<string, string>()
            {
                ["management:endpoints:enabled"] = "false",
                ["management:endpoints:path"] = "/cloudfoundryapplication",
                ["management:endpoints:loggers:enabled"] = "false",
                ["management:endpoints:heapdump:enabled"] = "true",
                ["management:endpoints:cloudfoundry:validatecertificates"] = "true",
                ["management:endpoints:cloudfoundry:enabled"] = "true"
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();

            var ep = new EnvEndpoint(opts, config, new TestHosting());
            var result = ep.DoInvoke(config);
            Assert.NotNull(result);
            Assert.Single(result.ActiveProfiles);
            Assert.Equal("EnvironmentName", result.ActiveProfiles[0]);
            Assert.Single(result.PropertySources);

            var desc = result.PropertySources[0];

            Assert.Equal("MemoryConfigurationProvider", desc.Name);
            var props = desc.Properties;
            Assert.NotNull(props);
            Assert.Equal(6, props.Count);
            Assert.Contains("management:endpoints:loggers:enabled", props.Keys);
            var prop = props["management:endpoints:loggers:enabled"];
            Assert.NotNull(prop);
            Assert.Equal("false", prop.Value);
            Assert.Null(prop.Origin);
        }

        [Fact]
        public void Sanitized_ReturnsExpected()
        {
            var opts = new EnvEndpointOptions();
            var appsettings = new Dictionary<string, string>()
            {
                ["password"] = "mysecret",
                ["secret"] = "mysecret",
                ["key"] = "mysecret",
                ["token"] = "mysecret",
                ["my_credentials"] = "mysecret",
                ["credentials_of"] = "mysecret",
                ["my_credentialsof"] = "mysecret",
                ["vcap_services"] = "mysecret"
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();

            var ep = new EnvEndpoint(opts, config, new TestHosting());
            var result = ep.DoInvoke(config);
            Assert.NotNull(result);

            var desc = result.PropertySources[0];

            Assert.Equal("MemoryConfigurationProvider", desc.Name);
            var props = desc.Properties;
            Assert.NotNull(props);
            foreach (var key in appsettings.Keys)
            {
                Assert.Contains(key, props.Keys);
                Assert.NotNull(props[key]);
                Assert.Equal("******", props[key].Value);
                Assert.Null(props[key].Origin);
            }
        }

        [Fact]
        public void Sanitized_NonDefault_WhenSet()
        {
            var appsettings = new Dictionary<string, string>()
            {
                ["management:endpoints:env:keystosanitize:0"] = "credentials",
                ["password"] = "mysecret"
            };

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(appsettings);
            var config = configurationBuilder.Build();
            var opts = new EnvEndpointOptions(config);
            var ep = new EnvEndpoint(opts, config, new TestHosting());
            var result = ep.DoInvoke(config);
            Assert.NotNull(result);

            var desc = result.PropertySources[0];
            Assert.Equal("MemoryConfigurationProvider", desc.Name);
            var props = desc.Properties;
            Assert.NotNull(props);
            Assert.Contains("password", props.Keys);
            Assert.NotNull(props["password"]);
            Assert.Equal("mysecret", props["password"].Value);
        }

        private class TestHosting : IHostingEnvironment
        {
            public string EnvironmentName { get => "EnvironmentName"; set => throw new NotImplementedException(); }

            public string ApplicationName { get => "ApplicationName"; set => throw new NotImplementedException(); }

            public string ContentRootPath { get => "ContentRootPath"; set => throw new NotImplementedException(); }

            public IFileProvider ContentRootFileProvider { get => null; set => throw new NotImplementedException(); }
        }
    }
}
