﻿using AntSK.Core.Repositories.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntSK.Domain.Repositories
{
    [Table("Kms")]
    public partial class Kmss : EntityBase
    {
        /// <summary>
        /// 图标
        /// </summary>
        [Required]
        public string Icon { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 会话模型
        /// </summary>
        [Required]
        public string Describe { get; set; }

        /// <summary>
        /// 会话模型ID
        /// </summary>
        [Required]
        public string? ChatModelID { get; set; }

        /// <summary>
        /// 向量模型ID
        /// </summary>
        [Required]
        public string? EmbeddingModelID { get; set; }

        /// <summary>
        /// 每个段落的最大标记数。
        /// </summary>
        public int MaxTokensPerParagraph { get; set; } = 299;

        /// <summary>
        /// 每行，也就是每句话的最大标记数
        /// </summary>
        public int MaxTokensPerLine { get; set; } = 99;

        /// <summary>
        /// 段落之间重叠标记的数量。
        /// </summary>
        public int OverlappingTokens { get; set; } = 49;
    }
}