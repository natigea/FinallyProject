namespace EcommersProject.BLL.DTOs;

public record ProductImageGetDto(Guid Id, Guid ProductId, string Url, bool IsPrimary);
public record ProductImageCreateDto(Guid ProductId, string Url, bool IsPrimary);
public record ProductImageUpdateDto(string Url, bool IsPrimary);
