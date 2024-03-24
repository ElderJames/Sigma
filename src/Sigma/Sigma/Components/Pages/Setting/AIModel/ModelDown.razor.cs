﻿using AntDesign;
using Sigma.Models;
using Sigma.Services;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using RestSharp;
using Sigma.Core.Utils;
using Sigma.Core.Domain.Model.hfmirror;

namespace Sigma.Components.Pages.Setting.AIModel
{
    public partial class ModelDown:ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }

        private readonly ListFormModel _model = new ListFormModel();
        private readonly IList<string> _selectCategories = new List<string>();

        private List<HfModels> _modelList = new List<HfModels>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            InitData("");
        }

        private void InitData(string searchKey)
        {
            var param = searchKey.ConvertToString().Split(" ");

            string urlBase = "https://hf-mirror.com/models-json?sort=trending&search=gguf";
            if (param.Count() > 0)
            {
                urlBase += "+" + string.Join("+", param);
            }
            RestClient client = new RestClient();
            RestRequest request = new RestRequest(urlBase, Method.Get);
            var response = client.Execute(request);
            var model = JsonConvert.DeserializeObject<HfModel>(response.Content);
            _modelList = model.models;
        }

        private async Task Search(string searchKey)
        {
            InitData(searchKey);
        }

        private void Down(string modelPath)
        {
            NavigationManager.NavigateTo($"/setting/modeldown/detail/{modelPath}");
        }
    }
}
