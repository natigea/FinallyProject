using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class CategoryService
    : GenericService<Category, CategoryGetDto, CategoryCreateDto, CategoryUpdateDto>, ICategoryService
{
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Categories) { }
}
