﻿
using AntSK.Domain.Domain.Interface;
using AntSK.Domain.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace AntSK.Domain.Domain.Other
{
    public class BackGroundTaskHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BackGroundTaskHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task ExecuteAsync(ImportKMSTaskReq item)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                Console.WriteLine("ExecuteAsync.开始执行后台任务");
                var importKMSService = scope.ServiceProvider.GetRequiredService<IImportKMSService>();
                //不能使用异步
                //importKMSService.ImportKMSTask(item);
                Console.WriteLine("ExecuteAsync.后台任务执行完成");
            }

        }

        public Task OnFailed()
        {
            return Task.CompletedTask;

        }

        public Task OnSuccess()
        {
            return Task.CompletedTask;
        }

    }
}
