using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwait.Task2.CodeReviewChallenge.Headers;
using CloudServices.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AsyncAwait.Task2.CodeReviewChallenge.Middleware
{
    public class StatisticMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IStatisticService _statisticService;

        public StatisticMiddleware(RequestDelegate next, IStatisticService statisticService)
        {
            _next = next;
            _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));
        }

        public async Task InvokeAsync(HttpContext context)
        {   
            string path = context.Request.Path;
                                                    // to async
            Task staticRegTask = Task.Run(
               async () =>  await _statisticService.RegisterVisitAsync(path)).ContinueWith(async o =>
               {
                   var result = await _statisticService.GetVisitsCountAsync(path);
                   context.Response.Headers.Add(CustomHttpHeaders.TotalPageVisits, result.ToString());
               }, TaskContinuationOptions.OnlyOnRanToCompletion);

            Console.WriteLine(staticRegTask.Status); // just for debugging purposes

            //Task staticRegTask = Task.Run(
            //    () => _statisticService.RegisterVisitAsync(path)
            //    .ConfigureAwait(false)
            //    .GetAwaiter().OnCompleted(UpdateHeaders));

            //Console.WriteLine(staticRegTask.Status); // just for debugging purposes
            
            //void UpdateHeaders()
            //{
            //    context.Response.Headers.Add(
            //        CustomHttpHeaders.TotalPageVisits,
            //        _statisticService.GetVisitsCountAsync(path).GetAwaiter().GetResult().ToString());
            //}

            Thread.Sleep(3000); // without this the statistic counter does not work
            await _next(context);
        }
    }
}
