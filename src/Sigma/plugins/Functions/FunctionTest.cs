﻿using Sigma.Core.Common;
using Sigma.Core.Repositories;

namespace Sigma.plugins.Functions
{
    public class FunctionTest(IAIModels_Repositories Repository)
    {
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="id">订单号</param>
        /// <returns>订单信息</returns>
        [SigmaFunction]
        public string GetOrder(int id)
        {
            return $"""
                    订单ID: {id}
                    商品名：小米MIX4
                    数量：1个
                    价格：4999元
                    收货地址：上海市黄浦区
                """;
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <returns>模型列表</returns>
        [SigmaFunction]
        public string GetModels()
        {
            var models = Repository.GetList();
            return string.Join(",", models.Select(x => x.ModelName));
        }
    }
}