namespace EcommersProject.BLL.DTOs;

public record CategoryGetDto(Guid Id, string Name, string Description);
public record CategoryCreateDto(string Name, string Description);
public record CategoryUpdateDto(string Name, string Description);
