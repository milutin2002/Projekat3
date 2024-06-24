using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Projekat3
{
    public class GitHubService
    {
        private readonly HttpClient _client;

        public GitHubService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("bearer", "personal_token");
            _client.DefaultRequestHeaders.Add("x-ratelimit-limit","12000");
        }

        public IObservable<RepoResult> GetRepositories(string language)
        {
            return Observable.FromAsync(() =>
                    _client.GetFromJsonAsync<GitHubRepositories>($"https://api.github.com/search/repositories?q=language:{language}")
                )
                .SubscribeOn(TaskPoolScheduler.Default)
                .SelectMany(root => root.items)
                .Select(repo => new RepoResult() { repo = repo.name, owner = repo.owner.login })
                .SelectMany(repo => GetCommitCount(repo));
        }
        private IObservable<RepoResult> GetCommitCount(RepoResult repo)
        {
            return Observable.Create<RepoResult>(async observer =>
            {
                string jsonContent="";
                int br = 0,totalCommits=0;
                try
                {
                    do
                    {
                        var request =
                             await _client.GetAsync(
                                $"https://api.github.com/repos/{repo.owner}/{repo.repo}/stats/commit_activity");
                        if (request.StatusCode == HttpStatusCode.OK)
                        {
                            jsonContent = await request.Content.ReadAsStringAsync();
                            var commitActivities = JsonSerializer.Deserialize<CommitActivity[]>(jsonContent);
                            totalCommits = commitActivities?.Sum(b => b.total) ?? 0;

                        }
                        br++;
                    } while (br < 5 && totalCommits == 0);
                    
                    repo.commits = totalCommits;
                    observer.OnNext(repo);
                    observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            }).SubscribeOn(TaskPoolScheduler.Default);
        }
        
    }
}