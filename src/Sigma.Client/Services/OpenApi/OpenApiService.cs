﻿using Sigma.Core.Domain.Interface;
using Sigma.Core.Domain.Model.Dto.OpenAPI;
using Sigma.Core.Domain.Model.Enum;
using Sigma.Core.Repositories;
using Sigma.Core.Utils;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Sigma.Services.OpenApi
{
    public interface IOpenApiService
    {
        Task Chat(OpenAIModel model, string sk, HttpContext HttpContext);
    }

    public class OpenApiService(
        IApps_Repositories _apps_Repositories,
        IKmss_Repositories _kmss_Repositories,
        IKmsDetails_Repositories _kmsDetails_Repositories,
        IKernelService _kernelService,
        IKMService _kMService,
        IChatService _chatService
        ) : IOpenApiService
    {
        public async Task Chat(OpenAIModel model, string sk, HttpContext HttpContext)
        {
            string headerValue = sk;
            Regex regex = new Regex(@"Bearer (.*)");
            Match match = regex.Match(headerValue);
            string token = match.Groups[1].Value;
            Apps app = _apps_Repositories.GetFirst(p => p.SecretKey == token);
            if (app.IsNotNull())
            {
                string msg = await HistorySummarize(app, model);
                switch (app.Type)
                {
                    case AppType.Chat:
                        //普通会话
                        if (model.stream)
                        {
                            OpenAIStreamResult result1 = new OpenAIStreamResult();
                            result1.created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            result1.choices = new List<StreamChoicesModel>() { new StreamChoicesModel() { delta = new OpenAIMessage() { role = "assistant" } } };
                            await SendChatStream(HttpContext, result1, app, msg);
                            HttpContext.Response.ContentType = "application/json";
                            await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result1));
                            await HttpContext.Response.CompleteAsync();
                            return;
                        }
                        else
                        {
                            OpenAIResult result2 = new OpenAIResult();
                            result2.created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            result2.choices = new List<ChoicesModel>() { new ChoicesModel() { message = new OpenAIMessage() { role = "assistant" } } };
                            result2.choices[0].message.content = await SendChat(msg, app);
                            HttpContext.Response.ContentType = "application/json";
                            await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result2));
                            await HttpContext.Response.CompleteAsync();
                        }
                        break;

                    case AppType.Kms:
                        //知识库问答
                        if (model.stream)
                        {
                            OpenAIStreamResult result3 = new OpenAIStreamResult();
                            result3.created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            result3.choices = new List<StreamChoicesModel>() { new StreamChoicesModel() { delta = new OpenAIMessage() { role = "assistant" } } };
                            await SendKmsStream(HttpContext, result3, app, msg);
                            HttpContext.Response.ContentType = "application/json";
                            await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result3));
                            await HttpContext.Response.CompleteAsync();
                        }
                        else
                        {
                            OpenAIResult result4 = new OpenAIResult();
                            result4.created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            result4.choices = new List<ChoicesModel>() { new ChoicesModel() { message = new OpenAIMessage() { role = "assistant" } } };
                            result4.choices[0].message.content = await SendKms(msg, app);
                            HttpContext.Response.ContentType = "application/json";
                            await HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result4));
                            await HttpContext.Response.CompleteAsync();
                        }
                        break;
                }
            }
        }

        private async Task SendChatStream(HttpContext HttpContext, OpenAIStreamResult result, Apps app, string msg)
        {
            HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");
            var chatResult = _chatService.SendChatByAppAsync(app, msg, "");
            int i = 0;
            await foreach (var content in chatResult)
            {
                result.choices[0].delta.content = content.ConvertToString();
                string message = $"data: {JsonConvert.SerializeObject(result)}\n\n";
                await HttpContext.Response.WriteAsync(message, Encoding.UTF8);
                await HttpContext.Response.Body.FlushAsync();
                //模拟延迟。
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            await HttpContext.Response.WriteAsync("data: [DONE]");
            await HttpContext.Response.Body.FlushAsync();

            await HttpContext.Response.CompleteAsync();
        }

        /// <summary>
        /// 发送普通对话
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="msg"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        private async Task<string> SendChat(string msg, Apps app)
        {
            string result = "";
            if (string.IsNullOrEmpty(app.Prompt) || !app.Prompt.Contains("{{$input}}"))
            {
                //如果模板为空，给默认提示词
                app.Prompt = app.Prompt.ConvertToString() + "{{$input}}";
            }
            var _kernel = _kernelService.GetKernelByApp(app);
            var temperature = app.Temperature / 100;//存的是0~100需要缩小
            OpenAIPromptExecutionSettings settings = new() { Temperature = temperature };

            await _kernelService.ImportFunctionsByApp(app, _kernel);
            settings.ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions;

            var promptTemplateFactory = new KernelPromptTemplateFactory();
            var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(app.Prompt));

            var func = _kernel.CreateFunctionFromPrompt(app.Prompt, settings);
            var chatResult = await _kernel.InvokeAsync(function: func, arguments: new KernelArguments() { ["input"] = msg });
            if (chatResult.IsNotNull())
            {
                string answers = chatResult.GetValue<string>();
                result = answers;
            }
            return result;
        }

        private async Task SendKmsStream(HttpContext HttpContext, OpenAIStreamResult result, Apps app, string msg)
        {
            HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");
            var chatResult = _chatService.SendKmsByAppAsync(app, msg, "");
            int i = 0;
            await foreach (var content in chatResult)
            {
                result.choices[0].delta.content = content.ConvertToString();
                string message = $"data: {JsonConvert.SerializeObject(result)}\n\n";
                await HttpContext.Response.WriteAsync(message, Encoding.UTF8);
                await HttpContext.Response.Body.FlushAsync();
                //模拟延迟。
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            await HttpContext.Response.WriteAsync("data: [DONE]");
            await HttpContext.Response.Body.FlushAsync();

            await HttpContext.Response.CompleteAsync();
        }

        /// <summary>
        /// 发送知识库问答
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="msg"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        private async Task<string> SendKms(string msg, Apps app)
        {
            var _kernel = _kernelService.GetKernelByApp(app);
            var _memory = _kMService.GetMemoryByKMS(app.KmsIdList.Split(",").FirstOrDefault());
            string result = "";
            //知识库问答
            var filters = new List<MemoryFilter>();

            var kmsidList = app.KmsIdList.Split(",");
            foreach (var kmsid in kmsidList)
            {
                filters.Add(new MemoryFilter().ByTag("kmsid", kmsid));
            }

            var xlresult = await _memory.SearchAsync(msg, index: "kms", filters: filters);
            string dataMsg = "";
            if (xlresult != null)
            {
                foreach (var item in xlresult.Results)
                {
                    foreach (var part in item.Partitions)
                    {
                        dataMsg += $"[file:{item.SourceName};Relevance:{(part.Relevance * 100).ToString("F2")}%]:{part.Text}{Environment.NewLine}";
                    }
                }
                KernelFunction jsonFun = _kernel.Plugins.GetFunction("KMSPlugin", "Ask");
                var chatResult = await _kernel.InvokeAsync(function: jsonFun,
                    arguments: new KernelArguments() { ["doc"] = dataMsg, ["history"] = "", ["questions"] = msg });
                if (chatResult.IsNotNull())
                {
                    string answers = chatResult.GetValue<string>();
                    result = answers;
                }
            }
            return result;
        }

        /// <summary>
        /// 历史会话的会话总结
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task<string> HistorySummarize(Apps app, OpenAIModel model)
        {
            var _kernel = _kernelService.GetKernelByApp(app);
            StringBuilder history = new StringBuilder();
            string questions = model.messages[model.messages.Count - 1].content;
            for (int i = 0; i < model.messages.Count() - 1; i++)
            {
                var item = model.messages[i];
                history.Append($"{item.role}:{item.content}{Environment.NewLine}");
            }

            if (model.messages.Count() > 10)
            {
                //历史会话大于10条，进行总结
                var msg = await _kernelService.HistorySummarize(_kernel, questions, history.ToString());
                return msg;
            }
            else
            {
                var msg = $"history：{history.ToString()}{Environment.NewLine} user：{questions}"; ;
                return msg;
            }
        }
    }
}