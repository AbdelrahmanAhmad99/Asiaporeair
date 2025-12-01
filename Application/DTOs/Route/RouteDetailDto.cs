using System.Collections.Generic;

namespace Application.DTOs.Route
{
    public class RouteDetailDto : RouteDto
    {
        public List<RouteOperatorDto> Operators { get; set; } = new List<RouteOperatorDto>();
    }
}