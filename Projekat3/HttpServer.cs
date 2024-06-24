using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Projekat3.Cache;

namespace Projekat3
{
    public class HttpServer
    {
        private HttpListener _httpListener;
        private GitHubService _gitHubService;
        private HttpClient _client;
        private CacheMemory _cacheMemory;

        public HttpServer()
        {
            _cacheMemory = new CacheMemory();
            _httpListener = new HttpListener();
            _gitHubService = new GitHubService();
            _httpListener.Prefixes.Add("http://localhost:8081/");
        }

        public async Task start()
        {
            _httpListener.Start();
            while (true)
            {
                var context = await _httpListener.GetContextAsync();
                _ = process(context);
            }
        }
        
        public async Task process(HttpListenerContext listenerContext)
        {
            var request = listenerContext.Request;
            var response = listenerContext.Response;
            try
            {
                
                var url = request.QueryString;
                var param = url.Get("jezik");
                Console.WriteLine(param);
                if (param!=null)
                {
                        var log = _cacheMemory.getLog(param);
                        if (log.Item1)
                        {
                            sendResponse(response, Encoding.ASCII.GetString(log.Item2.content), HttpStatusCode.OK);
                            return;
                        }

                        var observable = _gitHubService.GetRepositories(param);
                        ConcurrentBag<RepoResult> list = new ConcurrentBag<RepoResult>();
                        await observable.ForEachAsync(b =>
                        {
                            if (b.commits != 0)
                            {
                                list.Add(b);
                            }
                        });
                        var responseString = System.Text.Json.JsonSerializer.Serialize(list);
                        var bytes = Encoding.ASCII.GetBytes(responseString);
                        await sendResponseWithSave(response, bytes, HttpStatusCode.OK,param);
                }
                else
                {
                    await sendResponse(response, "Niste uneli parametar", HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                await sendResponse(response, "Niste uneli dobar parametar", HttpStatusCode.BadRequest);
            }
        }

        public async Task sendResponse(HttpListenerResponse response,string responseBody,HttpStatusCode code)
        {
            var bytes = Encoding.ASCII.GetBytes(responseBody);
            response.ContentType = "text";
            response.StatusCode = (int)code;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            response.Close();
            
        }
        public async Task sendResponseWithSave(HttpListenerResponse response,byte[] responseBody,HttpStatusCode code,string param)
        {
            var bytes = responseBody;
            response.ContentType = "text";
            response.StatusCode = (int)code;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            response.Close();
            if (code == HttpStatusCode.OK)
            {
                _cacheMemory.writeResponse(param, new Log(code, bytes.Length, "text", bytes));
            }
        }
        public async Task stop()
        {
            _httpListener.Close();
        }
    }
}