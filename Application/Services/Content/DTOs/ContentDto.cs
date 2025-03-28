﻿using Application.Services.Component.DTOs;

namespace Application.Services.Content.DTOs;

public class ContentDto
{
    public int? Id { get; set; }
    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public string Header { get; set; }
    public DateTime CreatedDate { get; set; }

    public List<ComponentDto> Components { get; set; } = new();
}