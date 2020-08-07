using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MmsReceiver
{
    public interface IApiClient
    {
        string PostRequest(string url, FormUrlEncodedContent postValue);
        string GetRequest(string url);
    }
}
