﻿using AntDesign;
using Sigma.Core.Repositories;
using Sigma.Models;
using Microsoft.AspNetCore.Components;

namespace Sigma.Components.Pages.Setting.AIModel
{
    public partial class ModelList
    {
        private readonly BasicListFormModel _model = new BasicListFormModel();

        private List<AIModels> _data;

        private string _searchKeyword;

        [Inject]
        protected IAIModels_Repositories _aIModels_Repositories { get; set; }

        [Inject]
        IConfirmService _confirmService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitData();
        }
        private async Task InitData(string searchKey = null)
        {
            if (string.IsNullOrEmpty(searchKey))
            {
                _data = _aIModels_Repositories.GetList();
            }
            else
            {
                _data = _aIModels_Repositories.GetList(p => p.ModelName.Contains(searchKey) || p.ModelDescription.Contains(searchKey));
            }
            await InvokeAsync(StateHasChanged);
        }
        public async Task OnSearch()
        {
            await InitData(_searchKeyword);
        }

        public async Task AddModel()
        {
            NavigationManager.NavigateTo("/setting/model/add");
        }

        public void Edit(string modelid)
        {
            NavigationManager.NavigateTo("/setting/model/add/" + modelid);
        }

        public async Task Delete(string modelid)
        {
            var content = "是否确认删除此模型";
            var title = "删除";
            var result = await _confirmService.Show(content, title, ConfirmButtons.YesNo);
            if (result == ConfirmResult.Yes)
            {
                await _aIModels_Repositories.DeleteAsync(modelid);
                await InitData("");
            }
        }
    }
}
