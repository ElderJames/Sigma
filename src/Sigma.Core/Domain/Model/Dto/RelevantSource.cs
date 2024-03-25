﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sigma.Core.Domain.Model.Dto
{
    public class RelevantSource
    {
        public string SourceName { get; set; }

        public string Text { get; set; }
        public float Relevance { get; set; }
        public override string ToString()
        {
            return $"[file:{SourceName};Relevance:{(Relevance * 100):F2}%]:{Text}";
        }
    }
}
