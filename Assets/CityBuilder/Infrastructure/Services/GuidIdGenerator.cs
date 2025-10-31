using System;
using CityBuilder.Application.Services;

namespace CityBuilder.Infrastructure.Services
{
    public class GuidIdGenerator : IIdGenerator
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}