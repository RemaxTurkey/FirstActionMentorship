using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.ComponentTypeAttribute.DTOs;

namespace Application.Services.ComponentType.DTOs
{
    public class ComponentTypeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<ComponentTypeAttributeDto> Attributes { get; set; }
    }
}