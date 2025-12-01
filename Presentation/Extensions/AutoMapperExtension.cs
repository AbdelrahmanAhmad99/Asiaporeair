using Application.DTOs.FlightSchedule;  
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AutoMapper;
using Application.Maps;
namespace Presentation.Extensions
{
    public static class AutoMapperExtension
    {
        public static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
        { 
            services.AddAutoMapper(cfg =>
            { 
                cfg.AddMaps(typeof(CreateFlightScheduleDto).Assembly);
            });
             
            services.AddControllers()
            .AddJsonOptions(options =>
            { 
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
             
            return services;
        }
    }
}