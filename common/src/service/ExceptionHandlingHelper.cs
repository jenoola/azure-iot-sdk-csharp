// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Common;
    using Microsoft.Azure.Devices.Common.Exceptions;
    using Microsoft.Azure.Devices.Common.Extensions;
    using Newtonsoft.Json.Linq;

    class ExceptionHandlingHelper
    {
        public static IDictionary<HttpStatusCode, Func<HttpResponseMessage, Task<Exception>>> GetDefaultErrorMapping()
        {
            var mappings = new Dictionary<HttpStatusCode, Func<HttpResponseMessage, Task<Exception>>>();

            mappings.Add(HttpStatusCode.NoContent, async (response) => new DeviceNotFoundException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.NotFound, async (response) =>
            {
                string exMsg = await GetExceptionMessageAsync(response).ConfigureAwait(false);
                string httpErrorCode = response.Headers.GetFirstValueOrNull(CommonConstants.HttpErrorCodeName);
                ErrorCode errorCode;
                if (Enum.TryParse(httpErrorCode, out errorCode))
                {
                    switch (errorCode)
                    {
                        case ErrorCode.DeviceNotOnline:
                            JObject jObj = JObject.Parse(exMsg);
                            jObj = jObj.GetValue("Message") as JObject;
                            if (jObj.Count > 0)
                            {
                                return new DeviceNotOnlineException(
                                    jObj.GetValue("message").Value<string>(),
                                    jObj.GetValue("trackingId").Value<string>(),
                                    jObj.GetValue("errorCode").Value<string>());
                            }
                            return new DeviceNotOnlineException(exMsg);
                    }
                }
                return new DeviceNotFoundException(exMsg);
            });
            mappings.Add(HttpStatusCode.Conflict, async (response) => new DeviceAlreadyExistsException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.BadRequest, async (response) => new ArgumentException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.Unauthorized, async (response) => new UnauthorizedException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.Forbidden, async (response) => new QuotaExceededException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.PreconditionFailed, async (response) => new DeviceMessageLockLostException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.RequestEntityTooLarge, async (response) => new MessageTooLargeException(await GetExceptionMessageAsync(response).ConfigureAwait(false))); ;
            mappings.Add(HttpStatusCode.InternalServerError, async (response) => new ServerErrorException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add(HttpStatusCode.ServiceUnavailable, async (response) => new ServerBusyException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));
            mappings.Add((HttpStatusCode)429, async (response) => new ThrottlingException(await GetExceptionMessageAsync(response).ConfigureAwait(false)));

            return mappings;
        }

        public static async Task<string> GetExceptionMessageAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
