﻿namespace Data.Entities;

public class ContentCategoryEmployeeAssoc : Entity
{
    public ContentCategory ContentCategory { get; set; }
    public int ContentCategoryId { get; set; }
    
    public int EmployeeId { get; set; }

    
    public bool IsCompleted { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}