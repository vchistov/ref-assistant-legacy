﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lardite.RefAssistant.ReflectionServices.DataAccess.Readers;

namespace Lardite.RefAssistant.ReflectionServices.Data
{
    public sealed class CustomAttributeInfo
    {
        internal CustomAttributeInfo(ICustomAttributeReader reader)
        {
            ThrowUtils.ArgumentNull(() => reader);

            this.AttributeType = reader.GetAttributeType();
            this.ConstructorArguments = reader.GetConstructorArguments();
            this.Fields = reader.GetFields();
            this.Properties = reader.GetProperties();
        }

        public TypeId AttributeType { get; private set; }

        public IEnumerable<TypeId> ConstructorArguments { get; private set; }

        public IEnumerable<TypeId> Fields { get; private set; }

        public IEnumerable<TypeId> Properties { get; private set; }
    }
}