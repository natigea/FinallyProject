namespace EcommersProject.BLL.DTOs;

public record BrandGetDto(Guid Id, string Name, string Description);
public record BrandCreateDto(string Name, string Description);
public record BrandUpdateDto(string Name, string Description);
