# IRepository

The `IRepository<T>` interface provides a generic repository pattern for data access operations in the GPU image processing library.

## API

### GetByIdAsync(Guid id)
Gets an entity by its ID.

### GetAllAsync()
Gets all entities.

### AddAsync(T entity)
Adds a new entity.

### UpdateAsync(T entity)
Updates an existing entity.

### DeleteAsync(Guid id)
Deletes an entity by ID.

### DeleteAsync(T entity)
Deletes an entity.

### ExistsAsync(Guid id)
Checks if an entity with the given ID exists.

### CountAsync()
Gets the total count of entities.

### GetPageAsync(int pageNumber, int pageSize)
Gets entities with pagination.

### SaveChangesAsync()
Saves all pending changes to the database.

## Usage

```csharp
// Example usage of IRepository for an ImageProcessingJob entity
public class ImageProcessingJobService
{
    private readonly IRepository<ImageProcessingJob> _jobRepository;

    public ImageProcessingJobService(IRepository<ImageProcessingJob> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<ImageProcessingJob?> GetJobById(Guid id)
    {
        return await _jobRepository.GetByIdAsync(id);
    }

    public async Task AddJob(ImageProcessingJob job)
    {
        await _jobRepository.AddAsync(job);
        await _jobRepository.SaveChangesAsync();
    }

    public async Task UpdateJobStatus(Guid id, string newStatus)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job != null)
        {
            job.Status = newStatus;
            await _jobRepository.UpdateAsync(job);
            await _jobRepository.SaveChangesAsync();
        }
    }
}
```