using Core.Interfaces.Data;
using Core.Interfaces.Factories;
using Infrastructure.Contexts;
using Infrastructure.Entities;

namespace Infrastructure.Repository;

public class ImageRepository(
    DataContext dataContext,
    IEntityFactory<Domain.Image?, ImageEntity> factory,
    IRepositoryResultFactory resultFactory
) : BaseRepository<Domain.Image, ImageEntity>(dataContext, factory, resultFactory), IImageRepository
{
    // If we want to override the methods from the BaseRepository
    // using override keyword, remember that the method needs to be virtual in the BaseRepository
}
