using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag?> GetTagByIdAsync(int id);
        Task<Tag> CreateTagAsync(Tag tag);
        Task<Tag?> UpdateTagAsync(int id, Tag tag);
        Task<bool> DeleteTagAsync(int id);
    }
}
