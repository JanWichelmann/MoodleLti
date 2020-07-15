using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;

namespace SampleToolProvider.Utilities
{
    // See https://stackoverflow.com/a/61263403
    // and https://stackoverflow.com/a/62899563
    public class CustomHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        public override string Name { get; set; }
        public override HttpMessageHandler PrimaryHandler { get; set; }
        public override IList<DelegatingHandler> AdditionalHandlers => new List<DelegatingHandler>();

        public override HttpMessageHandler Build()
        {
            return new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
        }
    }
}
