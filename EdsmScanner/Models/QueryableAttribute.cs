using System;

namespace EdsmScanner.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class QueryableAttribute : Attribute
    {
    }
}