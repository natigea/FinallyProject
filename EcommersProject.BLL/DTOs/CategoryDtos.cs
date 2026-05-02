namespace EcommersProject.BLL.DTOs;

public record CategoryGetDto(Guid Id, string Name, string Description, string Icon, string Slug = "");
public record CategoryCreateDto(string Name, string Description, string Icon = "bi-tag", string Slug = "");
public record CategoryUpdateDto(string Name, string Description, string Icon = "bi-tag", string Slug = "");
