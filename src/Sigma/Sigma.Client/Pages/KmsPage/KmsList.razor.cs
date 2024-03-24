﻿using AntDesign;
using Sigma.Core.Domain.Interface;
using Sigma.Core.Repositories;
using Microsoft.AspNetCore.Components;

namespace Sigma.Components.Pages
{
    public partial class KmsList
    {

        [Inject]
        protected IKmss_Repositories _kmss_Repositories { get; set; }
        [Inject]
        protected IKmsDetails_Repositories _kmsDetails_Repositories { get; set; }
        [Inject]
        IConfirmService _confirmService { get; set; }

        [Inject]
        protected IKMService _kMService { get; set; }

        private readonly ListGridType _listGridType = new ListGridType
        {
            Gutter = 16,
            Xs = 1,
            Sm = 2,
            Md = 3,
            Lg = 3,
            Xl = 4,
            Xxl = 4
        };
        private string deleteid { get; set; }
        private Kmss[] _data = { };


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitData("");
        }

        private async Task InitData(string searchKey)
        {
            var list = new List<Kmss> { new Kmss() };
            List<Kmss> data;
            if (string.IsNullOrEmpty(searchKey))
            {
                data = await _kmss_Repositories.GetListAsync();
            }
            else
            {
                data = await _kmss_Repositories.GetListAsync(p => p.Name.Contains(searchKey));
            }

            list.AddRange(data);
            _data = list.ToArray();
            await InvokeAsync(StateHasChanged);
        }

        private void NavigateToAddKms()
        {
            NavigationManager.NavigateTo("/kms/add");
        }

        private async Task Search(string searchKey)
        {
            await InitData(searchKey);
        }

        private void Info(string id)
        {
            NavigationManager.NavigateTo($"/kms/detail/{id}");
        }


        private async Task Update(string id)
        {
            NavigationManager.NavigateTo($"/kms/add/{id}");
        }
        private async Task Delete(string id)
        {

            var content = "删除知识库会一起删除导入的知识文档，无法还原。是否确认删除此知识库？";
            var title = "删除";
            var result = await _confirmService.Show(content, title, ConfirmButtons.YesNo);
            if (result == ConfirmResult.Yes)
            {
                var _memory = _kMService.GetMemoryByKMS(id);
                var detailList = _kmsDetails_Repositories.GetList(p => p.KmsId == id);
                foreach (var detail in detailList)
                {
                    var flag = await _kmsDetails_Repositories.DeleteAsync(detail.Id);
                    if (flag)
                    {
                        await _memory.DeleteDocumentAsync(index: "kms", documentId: detail.Id);
                    }
                }

                await _kmss_Repositories.DeleteAsync(id);
                await InitData("");
            }
        }
    }
}
