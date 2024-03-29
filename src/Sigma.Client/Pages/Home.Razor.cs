﻿using Sigma.Core.Domain.Model.Enum;
using Sigma.Core.Repositories;
using Sigma.Core.Utils;
using Microsoft.AspNetCore.Components;

namespace Sigma.Components.Pages
{
    public partial class Home: ComponentBase
    {
        [Inject]
        NavigationManager NavigationManager { get; set; }

        [Inject]
        IApps_Repositories _apps_Repositories { get; set; }
        [Inject]
        IAIModels_Repositories _aiModels_Repositories { get; set; }
        [Inject]
        IKmss_Repositories _kmss_Repositories { get; set; }
        [Inject]
        IKmsDetails_Repositories _kmsDetails_Repositories { get; set; }

        private string appCount;
        private string chatAppCount;
        private string kmsAppCount;

        private string kmsCount;

        private string fileCount;
        private string urlCount;
        private string textCount;
        private string filesCount;

        private string chatAIModelCount;
        private string embeddingAIModelCount;
        private string aiModelCount;

        private string imgSrc { get; set; }

        protected override async Task OnInitializedAsync()
        {
            chatAppCount = (await _apps_Repositories.CountAsync(p => p.Type == AppType.Chat)).ConvertToString();
            kmsAppCount = (await _apps_Repositories.CountAsync(p => p.Type == AppType.Kms)).ConvertToString();
            appCount= (chatAppCount.ConvertToInt32()+ kmsAppCount.ConvertToInt32()).ConvertToString();

            kmsCount =(await _kmss_Repositories.CountAsync(p=>true)).ConvertToString();

            chatAIModelCount = (await _aiModels_Repositories.CountAsync(p=>p.IsChat)).ConvertToString();
            embeddingAIModelCount = (await _aiModels_Repositories.CountAsync(p=>p.IsEmbedding)).ConvertToString();
            aiModelCount= (chatAIModelCount.ConvertToInt32() + embeddingAIModelCount.ConvertToInt32()).ConvertToString();

            fileCount=(await _kmsDetails_Repositories.CountAsync(p =>p.Type=="file")).ConvertToString();
            urlCount = (await _kmsDetails_Repositories.CountAsync(p => p.Type == "url")).ConvertToString();
            textCount = (await _kmsDetails_Repositories.CountAsync(p => p.Type == "text")).ConvertToString();
            filesCount = (fileCount.ConvertToInt32() + urlCount.ConvertToInt32() + textCount.ConvertToInt32()).ConvertToString();

        }

        private void NavToApp()
        {
            NavigationManager.NavigateTo("/AppList");

        }

        private void NavToKms()
        {
            NavigationManager.NavigateTo("/KmsList");
        }

        private void NavToAIModel()
        {
            NavigationManager.NavigateTo("/setting/modellist");
        }
    }
}
